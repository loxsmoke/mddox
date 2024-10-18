using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdDox.MarkdownFormatters
{
    public class GithubMarkdownFormatter : MarkdownFormatter
    {
        public override string Name => "github";

        public override string EscapeSpecialChars(string text)
        {
            if (text == null) return "";
            text = text.Replace("<", "\\<");
            text = text.Replace(">", "\\>");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&lt;", "<");
            text = text.Replace("|", "\\|");
            text = text.Replace(Environment.NewLine, "<br>");
            return text.Replace("\n", "<br>");
        }
    }
}
