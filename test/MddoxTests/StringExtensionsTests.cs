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
        [InlineData("#1", "aaa", "aaa")]
        [InlineData("#2", "aaa()", "aaa")]
        [InlineData("#3", "aaa(int t)", "aaaint-t")]
        [InlineData("#4", "aaa(int[] t)", "aaaint-t")]
        [InlineData("#5", "aaa(something<> t)", "aaasomething-t")]
        [InlineData("#6", "aaa(something<int> t)", "aaasomethingint-t")]
        [InlineData("#7", "CleanGenericTypeName(string genericTypeName)", "cleangenerictypenamestring-generictypename")]
        [InlineData("#8", "ToNameStringWithValueTupleNames(Type type, IList<string> tupleNames, Func<Type, Queue<string>, string> typeNameConverter, bool invokeTypeNameConverterForGenericType)",
            "tonamestringwithvaluetuplenamestype-type-iliststring-tuplenames-functype-queuestring-string-typenameconverter-bool-invoketypenameconverterforgenerictype")]

        [InlineData("#9", "IsNullable(Type type)", "isnullabletype-type")]
        [InlineData("#10", "ToNameString(Type type, Func<Type, string> typeNameConverter)", 
            "tonamestringtype-type-functype-string-typenameconverter")]
        [InlineData("#11", "ToNameString(Type type, Func<Type, Queue<string>, string> typeNameConverter, bool invokeTypeNameConverterForGenericType)",
            "tonamestringtype-type-functype-queuestring-string-typenameconverter-bool-invoketypenameconverterforgenerictype")]
        public void CleanupHeadingAnchor(string note, string text, string expectedResult)
        {
            var result = text.CleanupHeadingAnchor();
            Assert.Equal(expectedResult, result);
        }
    }
}
