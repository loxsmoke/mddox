using System;
using System.Collections.Generic;
using System.Text;

namespace MdDox.MarkdownFormatters.Interfaces
{
    public interface IMarkdownFormatter
    {
        /// <summary>
        /// The name of the markdown format.
        /// </summary>
        string Name { get; }

        #region Markdown format constants
        string NewLine { get; }
        string StartInlineCode { get; }
        string EndInlineCode { get; }
        string StartMultilineCode { get; }
        string EndMultilineCode { get; }
        #endregion

        /// <summary>
        /// Convert special characters into real equivalents. Can be markdown-dependent.
        /// Example: &lt; is <
        /// </summary>
        /// <param name="text">Text with special characters to replace.</param>
        /// <returns>Text with special characters replaced</returns>
        string EscapeSpecialChars(string text);

        /// <summary>
        /// Format text as heading.
        /// </summary>
        /// <param name="h">Heading level. Valid values 1..6</param>
        /// <param name="text">Heading text</param>
        /// <returns></returns>
        string Heading(int h, string text);
        string TableTitle(params string[] tableHeadings);
        string TableRow(params string[] texts);
        string Bold(string text);
        string Link(string anchorName, string text);
        string HeadingLink(string anchorName, string text = null);
        string WithNewline(string text);
    }
}
