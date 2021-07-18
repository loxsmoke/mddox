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
    }
}
