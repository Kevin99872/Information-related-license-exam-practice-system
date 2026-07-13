namespace B3.Models;

/// <summary>
/// 本機設定模型
/// </summary>
public class AppSettings
{
    /// <summary>AI 服務供應商 (Ollama / OpenAI / Claude)</summary>
    public string AiProvider { get; set; } = "Ollama";

    /// <summary>Ollama 端點</summary>
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";

    /// <summary>OpenAI 端點 (相容 API 亦可)</summary>
    public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1";

    /// <summary>OpenAI API Key</summary>
    public string OpenAiApiKey { get; set; } = string.Empty;

    /// <summary>OpenAI 模型名稱</summary>
    public string OpenAiModel { get; set; } = "gpt-4o-mini";

    /// <summary>Claude API Key</summary>
    public string ClaudeApiKey { get; set; } = string.Empty;

    /// <summary>Claude 模型名稱</summary>
    public string ClaudeModel { get; set; } = "claude-opus-4-8";

    /// <summary>Ollama 模型名稱</summary>
    public string OllamaModel { get; set; } = "llama3";

    /// <summary>是否使用本地 Transformers 模型</summary>
    public bool UseLocalTransformers { get; set; } = false;

    /// <summary>本地 Transformers 模型路徑或名稱（例如 local_model 或 huggingface repo 名稱）</summary>
    public string LocalTransformersModelPath { get; set; } = string.Empty;

    /// <summary>Python 執行路徑</summary>
    public string PythonPath { get; set; } = "python";

    /// <summary>C++ 編譯器路徑</summary>
    public string CppCompilerPath { get; set; } = "g++";

    /// <summary>DotNet 執行路徑</summary>
    public string DotNetPath { get; set; } = "dotnet";

    /// <summary>預設語言</summary>
    public string DefaultLanguage { get; set; } = "Python";

    /// <summary>每次練習題數</summary>
    public int QuestionsPerExam { get; set; } = 20;

    /// <summary>作答後立即顯示答案</summary>
    public bool ShowAnswerOnSubmit { get; set; } = true;

    /// <summary>倒數計時器</summary>
    public bool EnableCountdown { get; set; } = true;

    /// <summary>題目隨機排序</summary>
    public bool ShuffleQuestions { get; set; } = true;

    /// <summary>難度篩選</summary>
    public string Difficulty { get; set; } = "中等";

    /// <summary>介面主題名稱</summary>
    public string ThemeName { get; set; } = "藍色";

    /// <summary>介面字體</summary>
    public string FontFamily { get; set; } = "Segoe UI Variable Text, Microsoft JhengHei UI, Noto Sans TC";

    /// <summary>字體大小</summary>
    public int FontSize { get; set; } = 14;

    /// <summary>是否顯示進度條</summary>
    public bool ShowProgress { get; set; } = true;

    /// <summary>介面語言 (zh-TW / en-US)</summary>
    public string UiLanguage { get; set; } = "zh-TW";
}
