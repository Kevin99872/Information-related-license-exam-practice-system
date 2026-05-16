using B3.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace B3.Services;

/// <summary>
/// Ollama 服務
/// </summary>
public class OllamaService
{
    private readonly LocalSettingsService _settingsService = new();

    /// <summary>一般問答</summary>
    public async Task<string> AskAsync(string question)
    {
        var prompt = $"使用繁體中文回答。\n\n問題: {question}";
        return await GenerateAsync(prompt);
    }

    /// <summary>程式碼分析</summary>
    public async Task<string> AnalyzeCodeAsync(string code, string description)
    {
        var prompt = $"你是程式碼審查助手，請用繁體中文指出問題與改進建議。\n\n題目描述:\n{description}\n\n使用者程式碼:\n{code}";
        return await GenerateAsync(prompt);
    }

    /// <summary>呼叫 Ollama 生成</summary>
    private async Task<string> GenerateAsync(string prompt)
    {
        try
        {
            var settings = _settingsService.Load();

            // If user selected a local transformers model, try to call it via python
            if (settings.UseLocalTransformers && !string.IsNullOrWhiteSpace(settings.LocalTransformersModelPath))
            {
                return await GenerateWithLocalTransformersAsync(settings.PythonPath, settings.LocalTransformersModelPath, prompt);
            }

            var endpoint = settings.OllamaEndpoint.TrimEnd('/');
            var url = $"{endpoint}/api/generate";

            using var client = new HttpClient();
            var payload = new
            {
                model = settings.OllamaModel,
                prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(body);
            return result?.response?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Ollama 呼叫失敗: {ex.Message}";
        }
    }

    private async Task<string> GenerateWithLocalTransformersAsync(string pythonPath, string modelPath, string prompt)
    {
        try
        {
            var code = @"import sys
from transformers import pipeline
model = sys.argv[1] if len(sys.argv) > 1 else None
prompt = sys.stdin.read()
if model:
    gen = pipeline('text-generation', model=model)
else:
    gen = pipeline('text-generation')
out = gen(prompt, max_new_tokens=256, do_sample=False)[0].get('generated_text', '')
print(out)
";

            var tempPath = Path.Combine(Path.GetTempPath(), $"b3_tf_runner_{Guid.NewGuid():N}.py");
            await File.WriteAllTextAsync(tempPath, code, Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(pythonPath) ? "python" : pythonPath,
                Arguments = $"\"{tempPath}\" \"{modelPath}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                return "無法啟動本地 Python 進程。請確認 Python 可執行檔路徑設定正確。";
            }

            await proc.StandardInput.WriteAsync(prompt);
            proc.StandardInput.Close();

            var outputTask = proc.StandardOutput.ReadToEndAsync();
            var errorTask = proc.StandardError.ReadToEndAsync();

            var completed = await Task.WhenAny(Task.WhenAll(outputTask, errorTask), Task.Delay(30000));
            if (completed is Task delay && delay.Status == TaskStatus.RanToCompletion && !outputTask.IsCompleted)
            {
                try { proc.Kill(); } catch { }
                try { File.Delete(tempPath); } catch { }
                return "本地模型執行逾時。";
            }

            var output = await outputTask;
            var error = await errorTask;

            try { proc.WaitForExit(1000); } catch { }
            try { File.Delete(tempPath); } catch { }

            if (!string.IsNullOrWhiteSpace(error))
            {
                return $"本地模型執行錯誤: {error}";
            }

            return output?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"本地模型呼叫失敗: {ex.Message}";
        }
    }

    private class OllamaResponse
    {
        public string? response { get; set; }
    }
}
