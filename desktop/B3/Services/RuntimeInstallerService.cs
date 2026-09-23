using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using B3.Localization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace B3.Services;

/// <summary>可自動配置的執行環境</summary>
public enum RuntimeKind
{
    Python,
    Cpp,
    DotNet,
    Java
}

/// <summary>自動配置結果</summary>
public record RuntimeConfigureResult(bool Success, string? ExecutablePath, string Message);

/// <summary>
/// 執行環境自動配置服務 - 先偵測本機是否已安裝，找不到時透過套件管理員安裝
/// Windows: winget / macOS: Homebrew / Linux: apt-get (pkexec)
/// </summary>
public class RuntimeInstallerService
{
    private static readonly TimeSpan VersionCheckTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan InstallTimeout = TimeSpan.FromMinutes(30);

    /// <summary>偵測，必要時安裝，回傳可執行檔完整路徑</summary>
    public async Task<RuntimeConfigureResult> ConfigureAsync(RuntimeKind kind, string? currentPath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        progress?.Report(LocalizationService.T("RtDetecting"));
        var existing = await DetectAsync(kind, currentPath, cancellationToken);
        if (existing != null)
        {
            return new RuntimeConfigureResult(true, existing, string.Format(LocalizationService.T("RtFoundFmt"), existing));
        }

        var command = GetInstallCommand(kind);
        if (command == null)
        {
            return new RuntimeConfigureResult(false, null,
                string.Format(LocalizationService.T("RtUnsupportedFmt"), GetManualDownloadUrl(kind)));
        }

        if (!await IsCommandAvailableAsync(command.Value.FileName, command.Value.ProbeArgs, cancellationToken))
        {
            return new RuntimeConfigureResult(false, null,
                string.Format(LocalizationService.T("RtNoPackageManagerFmt"), command.Value.FileName, GetManualDownloadUrl(kind)));
        }

        progress?.Report(string.Format(LocalizationService.T("RtInstallingFmt"), command.Value.FileName, command.Value.Args));
        var (exitCode, output) = await RunAsync(command.Value.FileName, command.Value.Args, InstallTimeout, line => progress?.Report(line), cancellationToken);

        progress?.Report(LocalizationService.T("RtVerifying"));
        var installed = await DetectAsync(kind, currentPath, cancellationToken);
        if (installed != null)
        {
            return new RuntimeConfigureResult(true, installed, string.Format(LocalizationService.T("RtInstalledFmt"), installed));
        }

        var lastLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).LastOrDefault(l => l.Length > 0) ?? string.Empty;
        return new RuntimeConfigureResult(false, null,
            string.Format(LocalizationService.T("RtInstallFailedFmt"), exitCode, lastLine));
    }

    /// <summary>依序檢查目前設定、PATH 與常見安裝位置</summary>
    public async Task<string?> DetectAsync(RuntimeKind kind, string? currentPath, CancellationToken cancellationToken = default)
    {
        foreach (var candidate in GetCandidates(kind, currentPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await VerifyAsync(kind, candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetCandidates(RuntimeKind kind, string? currentPath)
    {
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        IEnumerable<string> Raw()
        {
            if (!string.IsNullOrWhiteSpace(currentPath))
            {
                yield return currentPath.Trim().Trim('"');
            }

            foreach (var name in GetCommandNames(kind))
            {
                foreach (var resolved in ResolveOnPath(name))
                {
                    yield return resolved;
                }
            }

            foreach (var known in GetKnownLocations(kind))
            {
                yield return known;
            }
        }

        foreach (var raw in Raw())
        {
            var path = ResolveOnPath(raw).FirstOrDefault() ?? raw;
            // Windows 的 WindowsApps\python.exe 是 Microsoft Store 捷徑，不是真正的直譯器
            if (OperatingSystem.IsWindows() && path.Contains(@"\WindowsApps\", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if ((Path.IsPathRooted(path) && !File.Exists(path)) || !seen.Add(path))
            {
                continue;
            }

            yield return path;
        }
    }

    private static string[] GetCommandNames(RuntimeKind kind) => kind switch
    {
        RuntimeKind.Python => OperatingSystem.IsWindows() ? new[] { "python", "py" } : new[] { "python3", "python" },
        RuntimeKind.Cpp => new[] { "g++" },
        RuntimeKind.DotNet => new[] { "dotnet" },
        RuntimeKind.Java => new[] { "java" },
        _ => Array.Empty<string>()
    };

    /// <summary>剛安裝完時目前程序的 PATH 尚未更新，因此補上常見安裝位置</summary>
    private static IEnumerable<string> GetKnownLocations(RuntimeKind kind)
    {
        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var wingetPackages = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");

            return kind switch
            {
                RuntimeKind.Python => Glob(Path.Combine(localAppData, "Programs", "Python"), "Python3*", "python.exe")
                    .Concat(Glob(programFiles, "Python3*", "python.exe"))
                    .Append(Path.Combine(localAppData, "Python", "bin", "python.exe")),
                RuntimeKind.Cpp => Glob(wingetPackages, "BrechtSanders.WinLibs*", Path.Combine("mingw64", "bin", "g++.exe"))
                    .Append(Path.Combine(localAppData, "Microsoft", "WinGet", "Links", "g++.exe"))
                    .Append(@"C:\msys64\ucrt64\bin\g++.exe")
                    .Append(@"C:\msys64\mingw64\bin\g++.exe"),
                RuntimeKind.DotNet => new[]
                {
                    Path.Combine(programFiles, "dotnet", "dotnet.exe"),
                    Path.Combine(userProfile, ".dotnet", "dotnet.exe")
                },
                RuntimeKind.Java => Glob(Path.Combine(programFiles, "Eclipse Adoptium"), "jdk-*", Path.Combine("bin", "java.exe"))
                    .Concat(Glob(Path.Combine(programFiles, "Microsoft"), "jdk-*", Path.Combine("bin", "java.exe")))
                    .Concat(Glob(Path.Combine(programFiles, "Java"), "jdk*", Path.Combine("bin", "java.exe"))),
                _ => Array.Empty<string>()
            };
        }

        if (OperatingSystem.IsMacOS())
        {
            return kind switch
            {
                RuntimeKind.Python => new[] { "/opt/homebrew/bin/python3", "/usr/local/bin/python3", "/usr/bin/python3" },
                RuntimeKind.Cpp => new[] { "/usr/bin/g++", "/opt/homebrew/bin/g++", "/usr/local/bin/g++" },
                RuntimeKind.DotNet => new[] { "/usr/local/share/dotnet/dotnet", "/opt/homebrew/bin/dotnet", "/usr/local/bin/dotnet" },
                RuntimeKind.Java => new[] { "/opt/homebrew/opt/openjdk@21/bin/java", "/usr/local/opt/openjdk@21/bin/java", "/usr/bin/java" },
                _ => Array.Empty<string>()
            };
        }

        return kind switch
        {
            RuntimeKind.Python => new[] { "/usr/bin/python3", "/usr/local/bin/python3" },
            RuntimeKind.Cpp => new[] { "/usr/bin/g++", "/usr/local/bin/g++" },
            RuntimeKind.DotNet => new[] { "/usr/bin/dotnet", "/usr/lib/dotnet/dotnet", "/usr/share/dotnet/dotnet" },
            RuntimeKind.Java => new[] { "/usr/bin/java" },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>列出 root 下符合 dirPattern 的子資料夾中的指定檔案 (新版本優先)</summary>
    private static IEnumerable<string> Glob(string root, string dirPattern, string relativeFile)
    {
        try
        {
            if (!Directory.Exists(root))
            {
                return Array.Empty<string>();
            }

            return Directory.GetDirectories(root, dirPattern)
                .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase)
                .Select(d => Path.Combine(d, relativeFile))
                .Where(File.Exists)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>在 PATH (含登錄檔中最新的使用者/系統 PATH) 中尋找指令</summary>
    private static IEnumerable<string> ResolveOnPath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            yield break;
        }

        if (Path.IsPathRooted(command) || command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
        {
            if (File.Exists(command))
            {
                yield return Path.GetFullPath(command);
            }
            yield break;
        }

        var pathValues = new List<string?> { Environment.GetEnvironmentVariable("PATH") };
        if (OperatingSystem.IsWindows())
        {
            // 安裝程式更新的是登錄檔中的 PATH，目前程序的環境變數不會自動刷新
            pathValues.Add(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User));
            pathValues.Add(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine));
        }

        var extensions = OperatingSystem.IsWindows() && !Path.HasExtension(command)
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)
            : new[] { string.Empty };

        var directories = pathValues
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .SelectMany(v => v!.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            .Select(d => Environment.ExpandEnvironmentVariables(d.Trim().Trim('"')))
            .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        foreach (var directory in directories)
        {
            foreach (var extension in extensions)
            {
                string full;
                try
                {
                    full = Path.Combine(directory, command + extension.ToLowerInvariant());
                }
                catch
                {
                    continue;
                }

                if (File.Exists(full))
                {
                    yield return full;
                }
            }
        }
    }

    /// <summary>執行版本指令確認環境真的可用</summary>
    private static async Task<bool> VerifyAsync(RuntimeKind kind, string executable, CancellationToken cancellationToken)
    {
        var args = kind switch
        {
            RuntimeKind.DotNet => "--list-sdks",
            RuntimeKind.Java => "-version",
            _ => "--version"
        };

        try
        {
            var (exitCode, output) = await RunAsync(executable, args, VersionCheckTimeout, null, cancellationToken);
            if (exitCode != 0)
            {
                return false;
            }

            return kind switch
            {
                RuntimeKind.Python => output.Contains("Python 3", StringComparison.OrdinalIgnoreCase),
                // 需要 SDK 才能編譯 C#，只有 runtime 不算
                RuntimeKind.DotNet => output.Split('\n').Any(line => line.Contains('[')),
                RuntimeKind.Java => output.Contains("version", StringComparison.OrdinalIgnoreCase),
                RuntimeKind.Cpp => output.Contains("g++", StringComparison.OrdinalIgnoreCase)
                                   || output.Contains("gcc", StringComparison.OrdinalIgnoreCase)
                                   || output.Contains("clang", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private readonly record struct InstallCommand(string FileName, string Args, string ProbeArgs);

    private static InstallCommand? GetInstallCommand(RuntimeKind kind)
    {
        if (OperatingSystem.IsWindows())
        {
            var id = kind switch
            {
                RuntimeKind.Python => "Python.Python.3.13",
                RuntimeKind.Cpp => "BrechtSanders.WinLibs.POSIX.UCRT",
                RuntimeKind.DotNet => "Microsoft.DotNet.SDK.10",
                RuntimeKind.Java => "EclipseAdoptium.Temurin.21.JDK",
                _ => null
            };
            return id == null
                ? null
                : new InstallCommand("winget", $"install --id {id} -e --silent --disable-interactivity --accept-source-agreements --accept-package-agreements", "--version");
        }

        if (OperatingSystem.IsMacOS())
        {
            return kind switch
            {
                RuntimeKind.Python => new InstallCommand("brew", "install python", "--version"),
                // macOS 的 g++ 由 Xcode Command Line Tools 提供 (會跳出系統安裝視窗)
                RuntimeKind.Cpp => new InstallCommand("xcode-select", "--install", "--version"),
                RuntimeKind.DotNet => new InstallCommand("brew", "install --cask dotnet-sdk", "--version"),
                RuntimeKind.Java => new InstallCommand("brew", "install openjdk@21", "--version"),
                _ => null
            };
        }

        if (OperatingSystem.IsLinux() && File.Exists("/usr/bin/apt-get"))
        {
            var package = kind switch
            {
                RuntimeKind.Python => "python3",
                RuntimeKind.Cpp => "g++",
                RuntimeKind.DotNet => "dotnet-sdk-10.0",
                RuntimeKind.Java => "openjdk-21-jdk",
                _ => null
            };
            return package == null ? null : new InstallCommand("pkexec", $"apt-get install -y {package}", "--version");
        }

        return null;
    }

    private static string GetManualDownloadUrl(RuntimeKind kind) => kind switch
    {
        RuntimeKind.Python => "https://www.python.org/downloads/",
        RuntimeKind.Cpp => "https://winlibs.com/",
        RuntimeKind.DotNet => "https://dotnet.microsoft.com/download",
        RuntimeKind.Java => "https://adoptium.net/",
        _ => string.Empty
    };

    private static async Task<bool> IsCommandAvailableAsync(string fileName, string probeArgs, CancellationToken cancellationToken)
    {
        var resolved = ResolveOnPath(fileName).FirstOrDefault();
        if (resolved == null)
        {
            return false;
        }

        try
        {
            var (exitCode, _) = await RunAsync(resolved, probeArgs, VersionCheckTimeout, null, cancellationToken);
            // pkexec --version 等指令部分版本回傳非 0，存在即視為可用
            return exitCode == 0 || fileName == "pkexec";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>執行外部程序並收集 stdout + stderr</summary>
    private static async Task<(int ExitCode, string Output)> RunAsync(string fileName, string args, TimeSpan timeout, Action<string>? onLine, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveOnPath(fileName).FirstOrDefault() ?? fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var output = new StringBuilder();
        var sync = new object();

        void Append(string? line)
        {
            if (line == null)
            {
                return;
            }

            lock (sync)
            {
                output.AppendLine(line);
            }

            // winget 會輸出進度條字元，過濾掉只剩符號的行
            var trimmed = line.Trim();
            if (onLine != null && trimmed.Any(char.IsLetterOrDigit))
            {
                onLine(trimmed);
            }
        }

        process.OutputDataReceived += (_, e) => Append(e.Data);
        process.ErrorDataReceived += (_, e) => Append(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // ignore
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            return (-1, "Timeout");
        }

        lock (sync)
        {
            return (process.ExitCode, output.ToString());
        }
    }}
