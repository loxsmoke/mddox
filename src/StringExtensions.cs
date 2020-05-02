using System.Text.RegularExpressions;

namespace MdDox
{
    internal static class StringExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replacement)
            => Regex.Replace(input, pattern, replacement);
    }
}
