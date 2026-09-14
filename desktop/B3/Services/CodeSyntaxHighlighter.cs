using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using B3.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace B3.Services;

/// <summary>語法 token 種類</summary>
public enum SyntaxTokenKind
{
    Keyword,
    Type,
    Function,
    String,
    Number,
    Comment,
    Preprocessor
}

/// <summary>語法 token 範圍</summary>
public readonly record struct SyntaxToken(int Start, int Length, SyntaxTokenKind Kind)
{
    public int End => Start + Length;
}

/// <summary>
/// 依程式語言切出語法 token - 支援 Python / C# / C++
/// 以整份文件掃描，因此可正確處理跨行註解與多行字串
/// </summary>
public static class CodeSyntaxTokenizer
{
    private sealed record LanguageRules(Regex Pattern, HashSet<string> Keywords, HashSet<string> Types);

    private static readonly LanguageRules PythonRules = new(
        new Regex(
            @"(?<comment>#[^\n]*)" +
            @"|(?<string>(?<![\w])[rRbBuUfF]{0,2}(?:""""""[\s\S]*?(?:""""""|$)|'''[\s\S]*?(?:'''|$)|""(?:\\.|[^""\\\n])*""?|'(?:\\.|[^'\\\n])*'?))" +
            @"|(?<preproc>(?m:^[ \t]*@[\w.]+))" +
            @"|(?<number>\b(?:0[xX][0-9A-Fa-f_]+|0[bB][01_]+|0[oO][0-7_]+|\d[\d_]*(?:\.\d*)?(?:[eE][+-]?\d+)?[jJ]?)\b)" +
            @"|(?<word>[A-Za-z_]\w*)",
            RegexOptions.Compiled),
        Set("False None True and as assert async await break class continue def del elif else except finally for from global if import in is lambda match case nonlocal not or pass raise return try while with yield"),
        Set("self cls int float str bool list dict set tuple bytes object type range print input len map filter zip enumerate sorted reversed sum min max abs round open iter next isinstance super Exception ValueError TypeError IndexError KeyError"));

    private static readonly LanguageRules CSharpRules = new(
        new Regex(
            @"(?<comment>//[^\n]*|/\*[\s\S]*?(?:\*/|$))" +
            @"|(?<string>(?:\$@|@\$|@)""(?:""""|[^""])*""?|\$?""(?:\\.|[^""\\\n])*""?|'(?:\\.|[^'\\\n])*'?)" +
            @"|(?<preproc>(?m:^[ \t]*#[ \t]*\w+))" +
            @"|(?<number>\b(?:0[xX][0-9A-Fa-f_]+|0[bB][01_]+|\d[\d_]*(?:\.\d+)?(?:[eE][+-]?\d+)?)(?:[uU][lL]?|[lL][uU]?|[fFdDmM])?\b)" +
            @"|(?<word>@?[A-Za-z_]\w*)",
            RegexOptions.Compiled),
        Set("abstract as base break case catch checked class const continue default delegate do else enum event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null operator out override params private protected public readonly record ref return sealed sizeof stackalloc static struct switch this throw true try typeof unchecked unsafe using virtual volatile while var async await get set init value yield nameof when where"),
        Set("bool byte sbyte char decimal double float int uint long ulong short ushort object string void dynamic String Console Math List Dictionary HashSet Queue Stack StringBuilder Convert Array Enumerable Int32 Int64 Double DateTime TimeSpan Exception Task Random"));

    private static readonly LanguageRules CppRules = new(
        new Regex(
            @"(?<comment>//[^\n]*|/\*[\s\S]*?(?:\*/|$))" +
            @"|(?<string>(?:u8|[uUL])?R""(?<delim>[^(\s""]{0,16})\([\s\S]*?(?:\)\k<delim>""|$)|(?:u8|[uUL])?""(?:\\.|[^""\\\n])*""?|(?:u8|[uUL])?'(?:\\.|[^'\\\n])*'?|(?<=#[ \t]*include[ \t]*)<[^>\n]*>)" +
            @"|(?<preproc>(?m:^[ \t]*#[ \t]*\w+))" +
            @"|(?<number>\b(?:0[xX][0-9A-Fa-f']+|0[bB][01']+|\d[\d']*(?:\.\d+)?(?:[eE][+-]?\d+)?)[uUlLfF]*\b)" +
            @"|(?<word>[A-Za-z_]\w*)",
            RegexOptions.Compiled),
        Set("alignas alignof auto break case catch class const constexpr const_cast continue decltype default delete do dynamic_cast else enum explicit export extern false for friend goto if inline mutable namespace new noexcept nullptr operator private protected public register reinterpret_cast return sizeof static static_cast struct switch template this throw true try typedef typeid typename union using virtual volatile while and or not"),
        Set("bool char char16_t char32_t wchar_t double float int long short signed unsigned void size_t int8_t int16_t int32_t int64_t uint8_t uint16_t uint32_t uint64_t std string vector map set multimap multiset unordered_map unordered_set pair tuple queue stack deque priority_queue list array bitset cin cout cerr endl"));

    private static HashSet<string> Set(string words) =>
        new(words.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);

    private static LanguageRules? GetRules(string? language) => language switch
    {
        "Python" => PythonRules,
        "C#" => CSharpRules,
        "C++" => CppRules,
        _ => null
    };

    /// <summary>切出整份文字的 token，依起點排序；不支援的語言回傳空清單</summary>
    public static List<SyntaxToken> Tokenize(string text, string? language)
    {
        var tokens = new List<SyntaxToken>();
        var rules = GetRules(language);
        if (rules == null || string.IsNullOrEmpty(text))
        {
            return tokens;
        }

        foreach (Match match in rules.Pattern.Matches(text))
        {
            if (match.Groups["comment"].Success)
            {
                tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Comment));
            }
            else if (match.Groups["string"].Success)
            {
                tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.String));
            }
            else if (match.Groups["preproc"].Success)
            {
                tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Preprocessor));
            }
            else if (match.Groups["number"].Success)
            {
                tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Number));
            }
            else if (match.Groups["word"].Success)
            {
                var word = match.Value.TrimStart('@');
                if (rules.Keywords.Contains(word))
                {
                    tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Keyword));
                }
                else if (rules.Types.Contains(word))
                {
                    tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Type));
                }
                else if (IsFollowedByParenthesis(text, match.Index + match.Length))
                {
                    tokens.Add(new SyntaxToken(match.Index, match.Length, SyntaxTokenKind.Function));
                }
            }
        }

        return tokens;
    }

    private static bool IsFollowedByParenthesis(string text, int index)
    {
        while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
        {
            index++;
        }

        return index < text.Length && text[index] == '(';
    }
}

/// <summary>
/// AvaloniaEdit 行上色器 - 將 token 以設定中的顏色繪製
/// </summary>
public class CodeSyntaxColorizer : DocumentColorizingTransformer
{
    private readonly Dictionary<SyntaxTokenKind, IBrush> _brushes = new();
    private List<SyntaxToken> _tokens = new();
    private TextDocument? _tokenizedDocument;
    private bool _isDirty = true;

    /// <summary>目前的程式語言 (null 表示不上色)</summary>
    public string? Language { get; private set; }

    public CodeSyntaxColorizer(SyntaxColorSettings colors)
    {
        ApplyColors(colors);
    }

    /// <summary>更新配色</summary>
    public void ApplyColors(SyntaxColorSettings colors)
    {
        var defaults = new SyntaxColorSettings();
        _brushes[SyntaxTokenKind.Keyword] = ToBrush(colors.Keyword, defaults.Keyword);
        _brushes[SyntaxTokenKind.Type] = ToBrush(colors.Type, defaults.Type);
        _brushes[SyntaxTokenKind.Function] = ToBrush(colors.Function, defaults.Function);
        _brushes[SyntaxTokenKind.String] = ToBrush(colors.String, defaults.String);
        _brushes[SyntaxTokenKind.Number] = ToBrush(colors.Number, defaults.Number);
        _brushes[SyntaxTokenKind.Comment] = ToBrush(colors.Comment, defaults.Comment);
        _brushes[SyntaxTokenKind.Preprocessor] = ToBrush(colors.Preprocessor, defaults.Preprocessor);
    }

    /// <summary>切換語言</summary>
    public void SetLanguage(string? language)
    {
        Language = language;
        Invalidate();
    }

    /// <summary>文件內容變更後需重新切 token</summary>
    public void Invalidate() => _isDirty = true;

    protected override void ColorizeLine(DocumentLine line)
    {
        var document = CurrentContext.Document;
        if (_isDirty || !ReferenceEquals(document, _tokenizedDocument))
        {
            _tokens = CodeSyntaxTokenizer.Tokenize(document.Text, Language);
            _tokenizedDocument = document;
            _isDirty = false;
        }

        if (_tokens.Count == 0 || line.Length == 0)
        {
            return;
        }

        var lineStart = line.Offset;
        var lineEnd = line.EndOffset;

        for (var i = FindFirstTokenEndingAfter(lineStart); i < _tokens.Count; i++)
        {
            var token = _tokens[i];
            if (token.Start >= lineEnd)
            {
                break;
            }

            var start = Math.Max(token.Start, lineStart);
            var end = Math.Min(token.End, lineEnd);
            if (end <= start)
            {
                continue;
            }

            var brush = _brushes[token.Kind];
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(brush));
        }
    }

    /// <summary>二分搜尋第一個結尾超過指定位置的 token (token 互不重疊，End 隨 Start 遞增)</summary>
    private int FindFirstTokenEndingAfter(int offset)
    {
        int low = 0, high = _tokens.Count;
        while (low < high)
        {
            var mid = (low + high) / 2;
            if (_tokens[mid].End <= offset)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return low;
    }

    /// <summary>將色碼轉為畫刷，格式錯誤時使用預設色</summary>
    public static IBrush ToBrush(string? hex, string fallbackHex) =>
        new SolidColorBrush(ParseColor(hex, fallbackHex));

    public static Color ParseColor(string? hex, string fallbackHex) =>
        !string.IsNullOrWhiteSpace(hex) && Color.TryParse(hex, out var color) ? color : Color.Parse(fallbackHex);
}

/// <summary>
/// 協助將語法上色套用到 TextEditor
/// </summary>
public static class CodeEditorHighlighting
{
    /// <summary>建立上色器並掛到編輯器，同時於內容變更時重新切 token</summary>
    public static CodeSyntaxColorizer Attach(TextEditor editor, SyntaxColorSettings colors)
    {
        var colorizer = new CodeSyntaxColorizer(colors);
        editor.TextArea.TextView.LineTransformers.Add(colorizer);
        editor.TextChanged += (_, _) => colorizer.Invalidate();
        editor.Foreground = CodeSyntaxColorizer.ToBrush(colors.PlainText, new SyntaxColorSettings().PlainText);
        return colorizer;
    }

    /// <summary>套用新配色 / 開關並重繪</summary>
    public static void Apply(TextEditor editor, CodeSyntaxColorizer colorizer, SyntaxColorSettings colors, string? language, bool enabled)
    {
        colorizer.ApplyColors(colors);
        colorizer.SetLanguage(enabled ? language : null);
        editor.Foreground = CodeSyntaxColorizer.ToBrush(colors.PlainText, new SyntaxColorSettings().PlainText);
        editor.TextArea.TextView.Redraw();
    }
}
