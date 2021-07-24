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

        protected virtual string EscapeSpecialChars(string text) => throw new NotImplementedException();

        protected virtual string EscapeSpecialText(string text)
        {
            if (text == null) return "";
            text = ResolveTag(text, "paramref", "name");
            return EscapeSpecialChars(text);
        }
        string ResolveTag(string text, string tagName, string attributeName)
        {
            var regex = new Regex("<" + tagName + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
            for (; ; )
            {
                var match = regex.Match(text);
                if (!match.Success) return text;

                var attributeValue = match.Groups[4].Value;
                text = text.Substring(0, match.Index) + Bold(attributeValue) + text.Substring(match.Index + match.Length);
            }
        }

        public virtual string Bold(string text)
        {
            return "**" + text + "**";
        }

        public virtual string Heading(int h, string text)
        {
            if (h <= 0 || h > 6) throw new ArgumentOutOfRangeException(nameof(h));
            return new string('#', h) + " " + EscapeSpecialChars(text) + Environment.NewLine + Environment.NewLine;
        }

        public virtual string HeadingLink(string anchorName, string text = null)
        {
            return $"[{text ?? anchorName}](#{anchorName.ToLower().RegexReplace(@"[^a-z\d -]", "").Replace(" ", "-")})";
        }

        public virtual string Link(string anchorName, string text)
        {
            return $"[{text}]({anchorName})";
        }

        public virtual string TableRow(params string[] texts)
        {
            return "| " + string.Join(" | ", texts.Select(EscapeSpecialText)) + " |" + Environment.NewLine;
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
