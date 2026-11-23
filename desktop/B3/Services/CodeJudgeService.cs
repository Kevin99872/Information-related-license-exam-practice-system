using B3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace B3.Services;

/// <summary>
/// 程式碼判題Service - 執行用戶程式碼並與預期輸出比對
/// 職責: 執行Python/C#程式碼 比較輸出結果 判斷答題正確性
/// </summary>
public class CodeJudgeService
{
    /// <summary>一般執行逾時 (毫秒)</summary>
    private const int RunTimeoutMs = 5000;

    /// <summary>含編譯步驟的逾時 (毫秒) - dotnet / g++ / java 編譯較久</summary>
    private const int CompiledTimeoutMs = 30000;

    /// <summary>原始碼以無 BOM 的 UTF-8 寫出 (BOM 會讓部分編譯器報錯)</summary>
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private readonly LocalSettingsService _settingsService = new();

    /// <summary>
    /// 執行Python程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecutePythonCodeAsync(string code, string input, IReadOnlyDictionary<string, string>? extraFiles = null)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var filePath = Path.Combine(workDir, "main.py");
        await File.WriteAllTextAsync(filePath, code, Utf8NoBom);
        await WriteExtraFilesAsync(workDir, extraFiles, "main.py");
        return await RunProcessAsync(settings.PythonPath, $"\"{filePath}\"", input, workDir);
    }

    /// <summary>
    /// 執行C#程式碼並返回輸出結果 (工作目錄內其他 .cs 檔會一併編譯)
    /// </summary>
    public async Task<string> ExecuteCSharpCodeAsync(string code, string input, IReadOnlyDictionary<string, string>? extraFiles = null)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var programPath = Path.Combine(workDir, "Program.cs");
        var projectPath = Path.Combine(workDir, "B3Run.csproj");

        await File.WriteAllTextAsync(programPath, code, Utf8NoBom);
        await File.WriteAllTextAsync(projectPath, BuildCSharpProject(), Utf8NoBom);
        await WriteExtraFilesAsync(workDir, extraFiles, "Program.cs", "B3Run.csproj");

        var args = $"run --project \"{projectPath}\"";
        return await RunProcessAsync(settings.DotNetPath, args, input, workDir, CompiledTimeoutMs);
    }

    /// <summary>
    /// 執行C++程式碼並返回輸出結果 (其他 .cpp / .cc / .c 檔會一併編譯)
    /// </summary>
    public async Task<string> ExecuteCppCodeAsync(string code, string input, IReadOnlyDictionary<string, string>? extraFiles = null)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var sourcePath = Path.Combine(workDir, "main.cpp");
        var exePath = Path.Combine(workDir, "main.exe");

        await File.WriteAllTextAsync(sourcePath, code, Utf8NoBom);
        var written = await WriteExtraFilesAsync(workDir, extraFiles, "main.cpp", "main.exe");

        var sources = new StringBuilder($"\"{sourcePath}\"");
        foreach (var name in written)
        {
            var ext = Path.GetExtension(name).ToLowerInvariant();
            if (ext is ".cpp" or ".cc" or ".cxx" or ".c")
            {
                sources.Append($" \"{Path.Combine(workDir, name)}\"");
            }
        }

        var compileArgs = $"{sources} -std=c++17 -O2 -o \"{exePath}\"";
        var compileOutput = await RunProcessAsync(settings.CppCompilerPath, compileArgs, string.Empty, workDir, CompiledTimeoutMs);
        if (!File.Exists(exePath))
        {
            return string.IsNullOrWhiteSpace(compileOutput) ? "編譯失敗" : compileOutput;
        }

        return await RunProcessAsync(exePath, string.Empty, input, workDir);
    }

    /// <summary>
    /// 執行Java程式碼並返回輸出結果 (單檔原始碼模式，需 Java 11 以上)
    /// </summary>
    public async Task<string> ExecuteJavaCodeAsync(string code, string input, IReadOnlyDictionary<string, string>? extraFiles = null)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var filePath = Path.Combine(workDir, "Main.java");
        await File.WriteAllTextAsync(filePath, code, Utf8NoBom);
        await WriteExtraFilesAsync(workDir, extraFiles, "Main.java");
        // 強制 Java 以 UTF-8 讀寫標準輸入 / 輸出 (預設為系統編碼，中文會變亂碼)
        var args = $"-Dfile.encoding=UTF-8 -Dstdout.encoding=UTF-8 -Dstderr.encoding=UTF-8 \"{filePath}\"";
        return await RunProcessAsync(settings.JavaPath, args, input, workDir, CompiledTimeoutMs);
    }

    /// <summary>
    /// 執行指定語言程式碼
    /// extraFiles: 主要程式檔以外的檔案 (檔名 → 內容)，會寫入同一工作目錄
    /// 連結: EditorWorkspace.ExtraFiles
    /// </summary>
    public async Task<string> ExecuteAsync(string language, string code, string input, IReadOnlyDictionary<string, string>? extraFiles = null)
    {
        try
        {
            return language switch
            {
                "Python" => await ExecutePythonCodeAsync(code, input, extraFiles),
                "C#" => await ExecuteCSharpCodeAsync(code, input, extraFiles),
                "C++" => await ExecuteCppCodeAsync(code, input, extraFiles),
                "Java" => await ExecuteJavaCodeAsync(code, input, extraFiles),
                _ => "不支援的語言"
            };
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            // 找不到直譯器 / 編譯器 (未安裝或設定頁路徑錯誤)
            return $"無法啟動 {language} 執行環境：{ex.Message}\n請至「設定」確認執行路徑。";
        }
    }

    /// <summary>
    /// 將額外檔案寫入工作目錄，回傳實際寫入的檔名
    /// reserved: 不可覆寫的檔名 (主要程式檔等)
    /// </summary>
    private static async Task<List<string>> WriteExtraFilesAsync(string workDir, IReadOnlyDictionary<string, string>? extraFiles, params string[] reserved)
    {
        var written = new List<string>();
        if (extraFiles == null)
        {
            return written;
        }

        foreach (var (rawName, content) in extraFiles)
        {
            // 只取檔名，避免寫到工作目錄之外
            var name = Path.GetFileName(rawName);
            if (string.IsNullOrWhiteSpace(name) ||
                Array.Exists(reserved, r => r.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            await File.WriteAllTextAsync(Path.Combine(workDir, name), content ?? string.Empty, Utf8NoBom);
            written.Add(name);
        }

        return written;
    }

    /// <summary>
    /// 比較實際輸出與期望輸出
    /// </summary>
    public bool CompareOutput(string actual, string expected)
    {
        // 標準化換行符和空白，並逐行比對
        var actualNorm = NormalizeOutput(actual);
        var expectedNorm = NormalizeOutput(expected);
        return string.Equals(actualNorm, expectedNorm, StringComparison.Ordinal);
    }

    /// <summary>
    /// 標準化輸出字符串
    /// </summary>
    private string NormalizeOutput(string output)
    {
        if (string.IsNullOrEmpty(output)) return string.Empty;
        // Split into lines, trim each line, collapse inner whitespace, then join with '\n'
        var lines = output.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var s = lines[i].Trim();
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
            lines[i] = s;
        }
        // remove empty trailing lines
        int last = lines.Length - 1;
        while (last >= 0 && string.IsNullOrEmpty(lines[last])) last--;
        if (last < 0) return string.Empty;
        return string.Join("\n", lines, 0, last + 1);
    }

    /// <summary>
    /// 建立暫存工作目錄
    /// </summary>
    private string CreateWorkDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "B3Exec", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 執行程序並返回輸出
    /// </summary>
    private async Task<string> RunProcessAsync(string fileName, string args, string input, string workingDir, int timeoutMs = RunTimeoutMs)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            // 統一以 UTF-8 收送資料，避免中文輸入 / 輸出變亂碼
            // (.NET 與 g++ 編出的程式本來就輸出 UTF-8；Python / Java 以下方設定強制改用 UTF-8)
            StandardInputEncoding = Utf8NoBom,
            StandardOutputEncoding = Utf8NoBom,
            StandardErrorEncoding = Utf8NoBom
        };
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
        startInfo.Environment["DOTNET_NOLOGO"] = "1";

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (!string.IsNullOrWhiteSpace(input))
        {
            await process.StandardInput.WriteAsync(input);
        }
        process.StandardInput.Close();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeout = new System.Threading.CancellationTokenSource(timeoutMs);
        var completed = true;
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            completed = false;
        }

        if (!completed)
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // ignore
            }
            return "執行逾時";
        }

        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            return string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim();
        }

        return output.Trim();
    }

    /// <summary>
    /// 產生 C# 專案檔
    /// </summary>
    private string BuildCSharpProject()
    {
        return """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
""";
    }
}
