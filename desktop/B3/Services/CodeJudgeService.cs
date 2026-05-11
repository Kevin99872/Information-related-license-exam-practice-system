using B3.Data;
using System.Threading.Tasks;

namespace B3.Services;

/// <summary>
/// 程式碼判題Service - 執行用戶程式碼並與預期輸出比對
/// 職責: 執行Python/C#程式碼 比較輸出結果 判斷答題正確性
/// </summary>
public class CodeJudgeService
{
    // TODO: 實現以下功能
    // 1. 支持Python程式碼執行
    // 2. 支持C#程式碼執行
    // 3. 輸出匹配邏輯
    // 4. 錯誤處理和超時檢測

    /// <summary>
    /// 執行Python程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecutePythonCodeAsync(string code, string input)
    {
        // TODO: 通過subprocess執行Python
        // 設定超時時間
        // 捕獲標準輸出和錯誤輸出
        return await Task.FromResult("");
    }

    /// <summary>
    /// 執行C#程式碼並返回輸出結果
    /// </summary>
    public async Task<string> ExecuteCSharpCodeAsync(string code, string input)
    {
        // TODO: 動態編譯C#
        // 執行並返回結果
        return await Task.FromResult("");
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
}
