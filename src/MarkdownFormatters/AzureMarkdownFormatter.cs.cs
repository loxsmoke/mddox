using System;

namespace MdDox.MarkdownFormatters
{
    /// <summary>
    /// Markdown formatter for Azure.
    /// </summary>
    public class AzureMarkdownFormatter : MarkdownFormatter 
    {
        /// <inheritdoc/>
        public override string Name => "azure";

        /// <inheritdoc/>
        public override string EscapeSpecialChars(string text)
        {
            if (text == null) return "";
            text = text.Replace("<", "\\<");
            text = text.Replace(">", "\\>");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&lt;", "<");
            text = text.Replace("|", "\\|");
            // Replace \r\n first (Windows), then \r alone (old Mac), but leave \n alone
            text = text.Replace("\r\n", "<br>");
            return text.Replace("\r", "<br>");
        }

        /// <inheritdoc/>
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
