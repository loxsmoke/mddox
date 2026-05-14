using MdDox.MarkdownFormatters.Interfaces;
using System;
using System.Linq;

namespace MdDox.MarkdownFormatters
{
    /// <summary>
    /// The base markdown formatter class. Provides common formatting functions.
    /// </summary>
    public abstract class MarkdownFormatter : IMarkdownFormatter
    {
        /// <summary>
        /// This property must be implemented by all markdown formatters. 
        /// It should return the name of the markdown format, e.g. "github", "azure", "bitbucket".
        /// </summary>
        public abstract string Name { get; }
        /// <inheritdoc/>
        public string NewLine => "<br>";
        /// <inheritdoc/>
        public string StartInlineCode => "`";
        /// <inheritdoc/>
        public string EndInlineCode => "`";
        /// <inheritdoc/>
        public string StartMultilineCode => "```" + Environment.NewLine;
        /// <inheritdoc/>
        public string EndMultilineCode => Environment.NewLine + "```" + Environment.NewLine;

        /// <summary>
        /// Escapes special markdown characters to prevent them from being interpreted as markdown syntax.
        /// Also converts XML entity references (like &amp;lt;, &amp;gt;) into their actual characters and then escapes them.
        /// Typically escapes characters like &lt;, &gt;, and | (pipe) with backslashes, and replaces newlines with &lt;br&gt; tags.
        /// The specific characters escaped may vary by markdown flavor implementation.
        /// </summary>
        /// <param name="text">The text containing special characters to escape. Can be null.</param>
        /// <returns>The text with special characters properly escaped for markdown, or an empty string if input is null.</returns>
        public abstract string EscapeSpecialChars(string text);

        /// <inheritdoc/>
        public virtual string Bold(string text)
        {
            return "**" + text + "**";
        }

        /// <inheritdoc/>
        public virtual string Heading(int headingLevel, string text)
        {
            if (headingLevel <= 0 || headingLevel > 6) throw new ArgumentOutOfRangeException(nameof(headingLevel));
            return new string('#', headingLevel) + " " + EscapeSpecialChars(text) + Environment.NewLine + Environment.NewLine;
        }

        /// <inheritdoc/>
        public virtual string HeadingLink(string anchorName, string text = null)
        {
            return $"[{text ?? anchorName}](#{anchorName.CleanupHeadingAnchor()})";
        }

        /// <inheritdoc/>
        public virtual string Link(string anchorName, string text)
        {
            return $"[{text}]({anchorName})";
        }

        /// <inheritdoc/>
        public virtual string TableRow(params string[] texts)
        {
            return "| " + string.Join(" | ", texts) + " |" + Environment.NewLine;
        }

        /// <inheritdoc/>
        public virtual string TableTitle(params string[] tableHeadings)
        {
            return "| " + string.Join(" | ", tableHeadings) + " |" + Environment.NewLine +
                   "|" + string.Join("|", tableHeadings.Select(x => "---")) + "|" + Environment.NewLine;
        }

        /// <inheritdoc/>
        public virtual string WithNewline(string text)
        {
            return text + Environment.NewLine + Environment.NewLine;
        }
    }
}
