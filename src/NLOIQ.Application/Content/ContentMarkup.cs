using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using NLOIQ.Application.Validation;
namespace NLOIQ.Application.Content;
// Content is text with a deliberately small formatting grammar, never trusted HTML.
public static partial class ContentMarkup
{
    [GeneratedRegex(@"\*\*([^*\r\n]+)\*\*|\*([^*\r\n]+)\*|\[([^\]\r\n]+)\]\(([^)\r\n]+)\)", RegexOptions.NonBacktracking, 1000)]
    private static partial Regex Tokens();
    private static string Inline(string text)
    {
        var result = new StringBuilder(); var cursor=0;
        foreach(Match match in Tokens().Matches(text)) {
            result.Append(HtmlEncoder.Default.Encode(text[cursor..match.Index]));
            if(match.Groups[1].Success) result.Append("<strong>"+HtmlEncoder.Default.Encode(match.Groups[1].Value)+"</strong>");
            else if(match.Groups[2].Success) result.Append("<em>"+HtmlEncoder.Default.Encode(match.Groups[2].Value)+"</em>");
            else if(ContentRules.IsSafeTarget(match.Groups[4].Value)) result.Append("<a href=\""+HtmlEncoder.Default.Encode(match.Groups[4].Value)+"\" rel=\"noopener noreferrer\">"+HtmlEncoder.Default.Encode(match.Groups[3].Value)+"</a>");
            else result.Append(HtmlEncoder.Default.Encode(match.Value));
            cursor=match.Index+match.Length;
        }
        return result.Append(HtmlEncoder.Default.Encode(text[cursor..])).ToString();
    }
    public static string Render(string? text)
    {
        var result=new StringBuilder(); var list=false;
        foreach(var line in (text ?? "").Replace("\r", "").Split('\n')) {
            if(line.StartsWith("- ",StringComparison.Ordinal)) { if(!list) {result.Append("<ul>");list=true;} result.Append("<li>"+Inline(line[2..])+"</li>"); }
            else { if(list){result.Append("</ul>");list=false;} if(!string.IsNullOrWhiteSpace(line)) result.Append("<p>"+Inline(line)+"</p>"); }
        }
        if(list)result.Append("</ul>"); return result.ToString();
    }
}
