namespace MdDox.MarkdownFormatters.Interfaces
{
    /// <summary>
    /// Interface for markdown formatters. 
    /// Implementations of this interface provide markdown formatting for different markdown flavors.
    /// </summary>
    public interface IMarkdownFormatter
    {
        /// <summary>
        /// The name of the markdown format.
        /// </summary>
        string Name { get; }

        #region Markdown format constants
        /// <summary>
        /// String for newline. Can be markdown-dependent. 
        /// </summary>
        string NewLine { get; }
        /// <summary>
        /// Inline code block start string.
        /// </summary>
        string StartInlineCode { get; }
        /// <summary>
        /// Inline code block end string.
        /// </summary>
        string EndInlineCode { get; }
        /// <summary>
        /// Multi-line code block start string.
        /// </summary>
        string StartMultilineCode { get; }
        /// <summary>
        /// Multi-line code block end string.
        /// </summary>
        string EndMultilineCode { get; }
        #endregion

        /// <summary>
        /// Convert special characters into real equivalents. Can be markdown-dependent.
        /// Example: &lt;
        /// </summary>
        /// <param name="text">Text with special characters to replace.</param>
        /// <returns>Text with special characters replaced</returns>
        string EscapeSpecialChars(string text);

        /// <summary>
        /// Format text as heading.
        /// </summary>
        /// <param name="h">Heading level. Valid values 1..6</param>
        /// <param name="text">Heading text</param>
        /// <returns>Markdown formatted heading</returns>
        string Heading(int h, string text);

        /// <summary>
        /// Write table title with specified column headings. 
        /// The method should also write the second line of the table with --- to separate the title from the table body.
        /// </summary>
        /// <param name="tableHeadings">Column headings for the table</param>
        /// <returns>Markdown formatted table title</returns>
        string TableTitle(params string[] tableHeadings);

        /// <summary>
        /// Creates a table row from the specified text values.
        /// </summary>
        /// <param name="texts">The text values for the table row cells.</param>
        /// <returns>A string representation of the table row.</returns>
        string TableRow(params string[] texts);

        /// <summary>
        /// Converts the specified text to bold formatting.
        /// </summary>
        /// <param name="text">The text to format as bold.</param>
        /// <returns>The text formatted in bold.</returns>
        string Bold(string text);

        /// <summary>
        /// Converts the specified anchor name and text into a markdown link. 
        /// The anchor name is the link target, and the text is the link text.
        /// </summary>
        /// <param name="anchorName">The name of the anchor.</param>
        /// <param name="text">The text to display for the link.</param>
        /// <returns>The formatted link</returns>
        string Link(string anchorName, string text);

        /// <summary>
        /// Converts the specified anchor name and text into a heading link in the same document.
        /// </summary>
        /// <param name="anchorName">The name of the anchor.</param>
        /// <param name="text">The text to display for the heading link.</param>
        /// <returns>The formatted heading link</returns>
        string HeadingLink(string anchorName, string text = null);

        /// <summary>
        /// Add newline(s) after the specified text. The number of newlines can be markdown-dependent. 
        /// For example, GitHub markdown requires two newlines to start a new paragraph.
        /// </summary>
        /// <param name="text">The text to which newlines will be added.</param>
        /// <returns>The text with added newlines.</returns>
        string WithNewline(string text);
    }
}
