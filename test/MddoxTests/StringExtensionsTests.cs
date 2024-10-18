using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static MdDox.StringExtensions;

namespace MddoxTests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, null, ".", null)]
        [InlineData(null, "", ".", "")]
        [InlineData("", null, ".", "")]
        [InlineData(null, "a", ".", "a")]
        [InlineData("a", null, ".", "a")]
        [InlineData("a", "b", ".", "a.b")]
        public void Add(string text, string add, string separator, string expectedResult)
        {
            var result = text.Add(add, separator);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("aaa",'*',"aaa")]
        [InlineData("aaa*",'*',"aaa*")]
        [InlineData("*",'*',"*")]
        [InlineData("**",'*',"*")]
        [InlineData("***",'*',"*")]
        [InlineData("*a*b*c",'*',"*a*b*c")]
        public void DedupChar(string text, char dchar, string expectedResult)
        {
            var result = text.DedupChar(dchar);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("aaa", "aaa")]
        [InlineData("aaa()", "aaa")]
        [InlineData("aaa(int t)", "aaa-int-t")]
        [InlineData("aaa(int[] t)", "aaa-int-t")]
        [InlineData("aaa(something<> t)", "aaa-something-t")]
        [InlineData("aaa(something<int> t)", "aaa-something-int-t")]
        public void CleanupHeadingAnchor(string text, string expectedResult)
        {
            var result = text.CleanupHeadingAnchor();
            Assert.Equal(expectedResult, result);
        }
    }
}
