using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Resources;

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
        public static CultureInfo culture = CultureInfo.CurrentCulture;
        public static string GetLocalized(this string str)
        {
            // 在这里使用 ResourceManager 或其他方式来获取对应语言的本地化字符串
            // 这里只是一个示例，实际实现取决于你的资源文件结构和加载方式
            // 假设你的资源文件是以键值对的形式存储的
            // 这里使用一个假设的 ResourceManager 来获取本地化字符串
            ResourceManager rm = new ResourceManager("MdDox.Resources.Resources", typeof(Program).Assembly);
            return rm.GetString(str, culture);
        }
    }
}
