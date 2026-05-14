namespace B3.Models;

/// <summary>
/// 本機設定模型
/// </summary>
public class AppSettings
{
    /// <summary>Ollama 端點</summary>
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";

    /// <summary>Ollama 模型名稱</summary>
    public string OllamaModel { get; set; } = "llama3";

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
}
