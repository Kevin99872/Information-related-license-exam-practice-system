using B3.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

    private class OllamaResponse
    {
        public string? response { get; set; }
    }
}
