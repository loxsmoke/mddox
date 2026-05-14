using System;

namespace MdDox.MarkdownFormatters
{
    /// <summary>
    /// Markdown formatter for GitHub.
    /// </summary>
    public class GithubMarkdownFormatter : MarkdownFormatter
    {
        /// <inheritdoc/>
        public override string Name => "github";

        /// <inheritdoc/>
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
