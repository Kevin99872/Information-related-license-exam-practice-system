using B3.Models;
using System;
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
    private readonly LocalSettingsService _settingsService = new();

    /// <summary>
    /// 執行Python程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecutePythonCodeAsync(string code, string input)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var filePath = Path.Combine(workDir, "main.py");
        await File.WriteAllTextAsync(filePath, code, Encoding.UTF8);
        return await RunProcessAsync(settings.PythonPath, $"\"{filePath}\"", input, workDir);
    }

    /// <summary>
    /// 執行C#程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecuteCSharpCodeAsync(string code, string input)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var programPath = Path.Combine(workDir, "Program.cs");
        var projectPath = Path.Combine(workDir, "B3Run.csproj");

        await File.WriteAllTextAsync(programPath, code, Encoding.UTF8);
        await File.WriteAllTextAsync(projectPath, BuildCSharpProject(), Encoding.UTF8);

        var args = $"run --project \"{projectPath}\"";
        return await RunProcessAsync(settings.DotNetPath, args, input, workDir);
    }

    /// <summary>
    /// 執行C++程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecuteCppCodeAsync(string code, string input)
    {
        var settings = _settingsService.Load();
        var workDir = CreateWorkDir();
        var sourcePath = Path.Combine(workDir, "main.cpp");
        var exePath = Path.Combine(workDir, "main.exe");

        await File.WriteAllTextAsync(sourcePath, code, Encoding.UTF8);
        var compileArgs = $"\"{sourcePath}\" -std=c++17 -O2 -o \"{exePath}\"";
        var compileOutput = await RunProcessAsync(settings.CppCompilerPath, compileArgs, string.Empty, workDir);
        if (!string.IsNullOrWhiteSpace(compileOutput))
        {
            return compileOutput;
        }

        return await RunProcessAsync(exePath, string.Empty, input, workDir);
    }

    /// <summary>
    /// 執行指定語言程式碼
    /// </summary>
    public async Task<string> ExecuteAsync(string language, string code, string input)
    {
        return language switch
        {
            "Python" => await ExecutePythonCodeAsync(code, input),
            "C#" => await ExecuteCSharpCodeAsync(code, input),
            "C++" => await ExecuteCppCodeAsync(code, input),
            _ => "不支援的語言"
        };
    }

    /// <summary>
    /// 比較實際輸出與期望輸出
    /// </summary>
    public bool CompareOutput(string actual, string expected)
    {
        // 標準化換行符和空白
        var actualNorm = NormalizeOutput(actual);
        var expectedNorm = NormalizeOutput(expected);
        return actualNorm == expectedNorm;
    }

    /// <summary>
    /// 標準化輸出字符串
    /// </summary>
    private string NormalizeOutput(string output)
    {
        return System.Text.RegularExpressions.Regex.Replace(output.Trim(), @"\s+", " ");
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
    private async Task<string> RunProcessAsync(string fileName, string args, string input, string workingDir)
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
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (!string.IsNullOrWhiteSpace(input))
        {
            await process.StandardInput.WriteAsync(input);
        }
        process.StandardInput.Close();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        var completed = process.WaitForExit(5000);
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
<Project Sdk=\"Microsoft.NET.Sdk\">
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
