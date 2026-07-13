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

    /// <summary>依設定的供應商呼叫 AI 生成</summary>
    private async Task<string> GenerateAsync(string prompt)
    {
        var settings = _settingsService.Load();

        return settings.AiProvider switch
        {
            "OpenAI" => await GenerateWithOpenAiAsync(settings, prompt),
            "Claude" => await GenerateWithClaudeAsync(settings, prompt),
            _ => await GenerateWithOllamaAsync(settings, prompt)
        };
    }

    /// <summary>呼叫 Ollama 生成</summary>
    private async Task<string> GenerateWithOllamaAsync(AppSettings settings, string prompt)
    {
        try
        {
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

    /// <summary>呼叫 OpenAI Chat Completions 生成 (相容 API 亦可)</summary>
    private async Task<string> GenerateWithOpenAiAsync(AppSettings settings, string prompt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(settings.OpenAiApiKey))
            {
                return "尚未設定 OpenAI API Key，請至 設定 > AI 模型 填入。";
            }

            var endpoint = settings.OpenAiEndpoint.TrimEnd('/');
            var url = $"{endpoint}/chat/completions";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.OpenAiApiKey);

            var payload = new
            {
                model = settings.OpenAiModel,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"OpenAI 呼叫失敗: {ex.Message}";
        }
    }

    /// <summary>呼叫 Claude Messages API 生成</summary>
    private async Task<string> GenerateWithClaudeAsync(AppSettings settings, string prompt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(settings.ClaudeApiKey))
            {
                return "尚未設定 Claude API Key，請至 設定 > AI 模型 填入。";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", settings.ClaudeApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var payload = new
            {
                model = settings.ClaudeModel,
                max_tokens = 4096,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            var sb = new StringBuilder();
            foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                {
                    sb.Append(block.GetProperty("text").GetString());
                }
            }
            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            return $"Claude 呼叫失敗: {ex.Message}";
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
