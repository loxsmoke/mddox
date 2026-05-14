using System;

namespace MdDox.MarkdownFormatters
{
    /// <summary>
    /// Markdown formatter for Bitbucket.
    /// </summary>
    public class BitbucketMarkdownFormatter : MarkdownFormatter
    {
        /// <inheritdoc/>
        public override string Name => "bitbucket";

        /// <inheritdoc/>
        public override string EscapeSpecialChars(string text)
        {
            if (text == null) return "";
            text = text.Replace("&gt;", ">");
            text = text.Replace("&lt;", "<");
            text = text.Replace("|", "\\|");
            text = text.Replace(Environment.NewLine, "<br>");
            return text.Replace("\n", "<br>");
        }

        /// <inheritdoc/>
        public override string HeadingLink(string anchorName, string text = null)
        {
            return $"[{text ?? anchorName}](#markdown-header-{anchorName.CleanupHeadingAnchor()})";
        }

        /// <inheritdoc/>
        public override string Heading(int h, string text)
        {
            return Environment.NewLine + base.Heading(h, text);
        }
    }
}
