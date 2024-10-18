using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdDox.MarkdownFormatters
{
    public class BitbucketMarkdownFormatter : MarkdownFormatter
    {
        public override string Name => "bitbucket";

        public override string EscapeSpecialChars(string text)
        {
            if (text == null) return "";
            text = text.Replace("&gt;", ">");
            text = text.Replace("&lt;", "<");
            text = text.Replace("|", "\\|");
            text = text.Replace(Environment.NewLine, "<br>");
            return text.Replace("\n", "<br>");
        }

        public override string HeadingLink(string anchorName, string text = null)
        {
            return $"[{text ?? anchorName}](#markdown-header-{anchorName.CleanupHeadingAnchor()})";
        }

        public override string Heading(int h, string text)
        {
            return Environment.NewLine + base.Heading(h, text);
        }
    }
}
