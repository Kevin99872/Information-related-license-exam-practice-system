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
}
