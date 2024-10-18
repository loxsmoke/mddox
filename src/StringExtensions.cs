using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MdDox
{
    public static class StringExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replacement)
            => Regex.Replace(input, pattern, replacement);

        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);
        
        public static string Add(this string existingText, string addText, string separator = " ")
        {
            if (addText == null) return existingText;
            if (existingText.IsNullOrEmpty()) return addText;
            return existingText + separator + addText;
        }

        public static bool EqualsIgnoreCase(this string text, string other) => 
            text.Equals(other, System.StringComparison.OrdinalIgnoreCase);

        public static string CleanupHeadingAnchor(this string anchor)
        {
            return anchor.ToLower()
                .Replace(@"()", "")
                .Replace(@"[]", "")
                .Replace(@"<>", "")
                .Replace(@">", "")
                .Replace(@")", "")
                .RegexReplace(@"[^a-z\d-]", "-");
        }

        /// <summary>
        /// Replace multiple occurrences of a character with a single occurrence.
        /// Example: char='*' text="a**b**c" -> "a*b*c"
        /// </summary>
        /// <param name="text"></param>
        /// <param name="charToDeduplicate"></param>
        /// <returns></returns>
        public static string DedupChar(this string text, char charToDeduplicate)
        {
            if (text.IsNullOrEmpty()) return text;
            var prevC = text[0];
            var newText = prevC.ToString();
            for (var i = 1; i < text.Length; i++)
            {
                if (text[i] == prevC && prevC == charToDeduplicate) continue;
                newText += text[i];
                prevC = text[i];
            }
            return newText;
        }

        /// <summary>
        /// Find tag and extract attribute value, inner text, and remaining text before and after attribute.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="tag">The name of the XML tag</param>
        /// <param name="attributeName">The name of the XML attribute</param>
        /// <returns></returns>
        public static (string attributeValue, string innerText, string beforeText, string afterText) FindTagWithAttribute(
            this string text, string tag, string attributeName)
        {
            // Check if tag is <tag attribute="value"/>
            (string attributeValue, string innerText, string beforeText, string afterText) =
                FindShortTagWithAttribute(text, tag, attributeName);
            if (attributeValue != null)
                return (attributeValue, innerText, beforeText, afterText);

            return FindLongTagWithAttribute(text, tag, attributeName);
        }

        public static (string attributeValue, string innerText, string beforeText, string afterText) FindShortTagWithAttribute(
            this string text, string tag, string attributeName)
        {
            // If text does not contain the tag then return the same text
            if (text.IsNullOrEmpty() || !text.Contains(tag)) return (null, null, text, null);

            // Check if tag is <tag attribute="value"/>
            var simpleTag = new Regex("<" + tag + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
            var match = simpleTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, "", text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }

            return (null, null, text, null);
        }

        public static (string attributeValue, string innerText, string beforeText, string afterText) FindLongTagWithAttribute(
            this string text, string tag, string attributeName = null)
        {
            // If text does not contain the tag then return the same text
            if (text.IsNullOrEmpty() || !text.Contains(tag)) return (null, null, text, null);

            // Check if tag is <tag attribute="value">inner text</tag>
            var bigTag = attributeName.IsNullOrEmpty() ?
                new Regex($"<{tag}>(.*?)</{tag}>") :
                new Regex($"<{tag}( +){attributeName}( *)=( *)\"(.*?)\"( *)>(.*?)</{tag}>");
            int valueMatchIndex = attributeName.IsNullOrEmpty() ? 1 : 6;
            var match = bigTag.Match(text);
            if (match.Success)
            {
                return (attributeName.IsNullOrEmpty() ? null : match.Groups[4].Value,
                    match.Groups[valueMatchIndex].Value,
                    text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }
            return (null, null, text, null);
        }

    }
}
