using MdDox;
using MdDox.MarkdownFormatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdDox.MarkdownFormatters
{
    public class AzureMarkdownFormatter : MarkdownFormatter 
    {
        public override string Name => "azure";

        protected override string EscapeSpecialChars(string text)
        {
            if (text == null) return "";
            text = text.Replace("<", "\\<");
            text = text.Replace(">", "\\>");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&lt;", "<");
            text = text.Replace("|", "\\|");
            return text.Replace(Environment.NewLine, "<br>");
        }

        public override string HeadingLink(string anchorName, string text = null)
        {
            anchorName = anchorName.ToLower()
                .Replace("<", "%5C%3C")
                .Replace(">", "%5C%3E")
                .Replace(",", "%2C");
            anchorName = anchorName.RegexReplace(@"[^a-zA-Z\d\(\)\% -]", "").Replace(" ", "-");
            return $"[{text ?? anchorName}](#{anchorName})";
        }
    }
}
