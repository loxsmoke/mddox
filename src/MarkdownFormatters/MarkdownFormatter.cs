using MdDox.MarkdownFormatters.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdDox.MarkdownFormatters
{
    public class MarkdownFormatter : IMarkdownFormatter
    {
        public virtual string Name => throw new NotImplementedException();
        public string NewLine => "<br>";
        public string StartInlineCode => "`";
        public string EndInlineCode => "`";
        public string StartMultilineCode => "```" + Environment.NewLine;
        public string EndMultilineCode => Environment.NewLine + "```" + Environment.NewLine;

        public virtual string EscapeSpecialChars(string text) => throw new NotImplementedException();

        public virtual string Bold(string text)
        {
            return "**" + text + "**";
        }

        public virtual string Heading(int headingLevel, string text)
        {
            if (headingLevel <= 0 || headingLevel > 6) throw new ArgumentOutOfRangeException(nameof(headingLevel));
            return new string('#', headingLevel) + " " + EscapeSpecialChars(text) + Environment.NewLine + Environment.NewLine;
        }

        public virtual string HeadingLink(string anchorName, string text = null)
        {
            return $"[{text ?? anchorName}](#{anchorName.CleanupHeadingAnchor()})";
        }

        public virtual string Link(string anchorName, string text)
        {
            return $"[{text}]({anchorName})";
        }

        public virtual string TableRow(params string[] texts)
        {
            return "| " + string.Join(" | ", texts) + " |" + Environment.NewLine;
        }

        public virtual string TableTitle(params string[] tableHeadings)
        {
            return "| " + string.Join(" | ", tableHeadings) + " |" + Environment.NewLine +
                   "|" + string.Join("|", tableHeadings.Select(x => "---")) + "|" + Environment.NewLine;
        }

        public virtual string WithNewline(string text)
        {
            return text + Environment.NewLine + Environment.NewLine;
        }
    }
}
