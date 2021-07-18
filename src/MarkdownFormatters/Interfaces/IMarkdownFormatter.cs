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
        /// Format text as heading.
        /// </summary>
        /// <param name="h">Heading level. Valid values 1..6</param>
        /// <param name="text"></param>
        /// <returns></returns>
        string Heading(int h, string text);
        string TableTitle(params string[] tableHeadings);
        string TableRow(params string[] texts);
        string Bold(string text);
        string Link(string anchorName, string text);
        string HeadingLink(string anchorName, string text);
        string WithNewline(string text);
    }
}
