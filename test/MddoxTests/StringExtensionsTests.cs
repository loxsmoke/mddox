using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MdDox.StringExtensions;

namespace MddoxTests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [DataTestMethod]
        [DataRow(null, null, ".", null)]
        [DataRow(null, "", ".", "")]
        [DataRow("", null, ".", "")]
        [DataRow(null, "a", ".", "a")]
        [DataRow("a", null, ".", "a")]
        [DataRow("a", "b", ".", "a.b")]
        public void Add(string text, string add, string separator, string expectedResult)
        {
            var result = text.Add(add, separator);
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow("aaa",'*',"aaa")]
        [DataRow("aaa*",'*',"aaa*")]
        [DataRow("*",'*',"*")]
        [DataRow("**",'*',"*")]
        [DataRow("***",'*',"*")]
        [DataRow("*a*b*c",'*',"*a*b*c")]
        public void DedupChar(string text, char dchar, string expectedResult)
        {
            var result = text.DedupChar(dchar);
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow("aaa", "aaa", DisplayName = "#01 aaa")]
        [DataRow("aaa()", "aaa", DisplayName = "#02 aaa()")]
        [DataRow("aaa(int t)", "aaaint-t", DisplayName = "#03 aaa(int t)")]
        [DataRow("aaa(int[] t)", "aaaint-t", DisplayName = "#04 aaa(int[] t)")]
        [DataRow("aaa(something<> t)", "aaasomething-t", DisplayName = "#05 aaa(something<> t)")]
        [DataRow("aaa(something<int> t)", "aaasomethingint-t", DisplayName = "#06 aaa(something<int> t)")]
        [DataRow("CleanGenericTypeName(string genericTypeName)", "cleangenerictypenamestring-generictypename", DisplayName = "#07 CleanGenericTypeName(string genericTypeName)")]
        [DataRow("ToNameStringWithValueTupleNames(Type type, IList<string> tupleNames, Func<Type, Queue<string>, string> typeNameConverter, bool invokeTypeNameConverterForGenericType)",
            "tonamestringwithvaluetuplenamestype-type-iliststring-tuplenames-functype-queuestring-string-typenameconverter-bool-invoketypenameconverterforgenerictype", 
            DisplayName = "#08 function with multiple parameters and generic types")]

        [DataRow("IsNullable(Type type)", "isnullabletype-type", DisplayName = "#09 IsNullable(Type type)")]
        [DataRow("ToNameString(Type type, Func<Type, string> typeNameConverter)", 
            "tonamestringtype-type-functype-string-typenameconverter", DisplayName = "#10 function with multiple parameters and generic types")]
        [DataRow("ToNameString(Type type, Func<Type, Queue<string>, string> typeNameConverter, bool invokeTypeNameConverterForGenericType)",
            "tonamestringtype-type-functype-queuestring-string-typenameconverter-bool-invoketypenameconverterforgenerictype", 
            DisplayName = "#11 function with multiple parameters and generic types")]
        public void CleanupHeadingAnchor(string text, string expectedResult)
        {
            var result = text.CleanupHeadingAnchor();
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(null, "code", null, 
            null, null, null, null, DisplayName = "Null text returns null")]
        [DataRow("", "code", null, 
            null, null, "", null, DisplayName = "Empty text returns empty before text")]
        [DataRow("Some text without tags", "code", null, 
            null, null, "Some text without tags", null, DisplayName = "No matching tag returns original text")]
        [DataRow("Before <code>var x = 5;</code> After", "code", null, 
            null, "var x = 5;", "Before ", " After", DisplayName = "Simple tag returns inner text")]
        [DataRow("<code>console.log('test');</code> After", "code", null, 
            null, "console.log('test');", "", " After", DisplayName = "Tag at start returns empty before text")]
        [DataRow("Before <code>int i = 0;</code>", "code", null, 
            null, "int i = 0;", "Before ", "", DisplayName = "Tag at end returns empty after text")]
        [DataRow("Before <code lang=\"csharp\">public class Test { }</code> After", "code", "lang", 
            "csharp", "public class Test { }", "Before ", " After", DisplayName = "Tag with attribute returns attribute value and inner text")]
        [DataRow("Before <code>line1\nline2\nline3</code> After", "code", null, 
            null, null, "Before <code>line1\nline2\nline3</code> After", null, DisplayName = "Multiline content does not match (regex limitation)")]
        [DataRow("Before <code>single line content</code> After", "code", null, 
            null, "single line content", "Before ", " After", DisplayName = "Single line content returns inner text")]
        [DataRow("Some <c>inline code</c> text", "c", null, 
            null, "inline code", "Some ", " text", DisplayName = "Different tag name finds correct tag")]
        [DataRow("Before <code  lang  =  \"python\"  >print('hello')</code> After", "code", "lang", 
            "python", "print('hello')", "Before ", " After", DisplayName = "Attribute with various spacing works")]
        [DataRow("Before <code></code> After", "code", null, 
            null, "", "Before ", " After", DisplayName = "Empty tag content returns empty inner text")]
        [DataRow("First <code>code1</code> Second <code>code2</code>", "code", null, 
            null, "code1", "First ", " Second <code>code2</code>", DisplayName = "Returns first occurrence only")]
        [DataRow("Before <code>&lt;div&gt;test&lt;/div&gt;</code> After", "code", null, 
            null, "&lt;div&gt;test&lt;/div&gt;", "Before ", " After", DisplayName = "Preserves special characters")]
        [DataRow("Before <code lang=\"csharp\">var x = 5;</code> After", "code", "type", 
            null, null, "Before <code lang=\"csharp\">var x = 5;</code> After", null, DisplayName = "Wrong attribute name returns null")]
        public void FindLongTagWithAttribute(string text, string tagName, string attributeName, 
            string expectedAttributeValue, string expectedInnerText, string expectedBeforeText, string expectedAfterText)
        {
            var (attributeValue, innerText, beforeText, afterText) = text.FindLongTagWithAttribute(tagName, attributeName);

            Assert.AreEqual(expectedAttributeValue, attributeValue);
            Assert.AreEqual(expectedInnerText, innerText);
            Assert.AreEqual(expectedBeforeText, beforeText);
            Assert.AreEqual(expectedAfterText, afterText);
        }

        [DataTestMethod]
        [DataRow(null, "paramref", "name", 
            null, null, null, null, DisplayName = "Null text returns null")]
        [DataRow("", "paramref", "name", 
            null, null, "", null, DisplayName = "Empty text returns empty before text")]
        [DataRow("Some text without tags", "paramref", "name", 
            null, null, "Some text without tags", null, DisplayName = "No matching tag returns original text")]
        [DataRow("Before <paramref name=\"value\"/> After", "paramref", "name", 
            "value", "", "Before ", " After", DisplayName = "Simple self-closing tag with attribute")]
        [DataRow("<paramref name=\"value\"/> After", "paramref", "name", 
            "value", "", "", " After", DisplayName = "Tag at start returns empty before text")]
        [DataRow("Before <paramref name=\"value\"/>", "paramref", "name", 
            "value", "", "Before ", "", DisplayName = "Tag at end returns empty after text")]
        [DataRow("Before <paramref  name  =  \"param1\"  /> After", "paramref", "name", 
            "param1", "", "Before ", " After", DisplayName = "Various spacing around attribute")]
        [DataRow("Before <paramref name=\"first\"/> Middle <paramref name=\"second\"/> After", "paramref", "name", 
            "first", "", "Before ", " Middle <paramref name=\"second\"/> After", DisplayName = "Returns first occurrence only")]
        [DataRow("Before <paramref type=\"value\"/> After", "paramref", "name", 
            null, null, "Before <paramref type=\"value\"/> After", null, DisplayName = "Wrong attribute name returns null")]
        [DataRow("Before <see cref=\"MyType\"/> After", "see", "cref", 
            "MyType", "", "Before ", " After", DisplayName = "Different tag name with attribute")]
        [DataRow("Before <paramref name=\"value&amp;more\"/> After", "paramref", "name", 
            "value&amp;more", "", "Before ", " After", DisplayName = "Preserves special characters in attribute")]
        public void FindShortTagWithAttribute(string text, string tagName, string attributeName,
            string expectedAttributeValue, string expectedInnerText, string expectedBeforeText, string expectedAfterText)
        {
            var (attributeValue, innerText, beforeText, afterText) = text.FindShortTagWithAttribute(tagName, attributeName);

            Assert.AreEqual(expectedAttributeValue, attributeValue);
            Assert.AreEqual(expectedInnerText, innerText);
            Assert.AreEqual(expectedBeforeText, beforeText);
            Assert.AreEqual(expectedAfterText, afterText);
        }

        [DataTestMethod]
        [DataRow(null, "tag", "attr", 
            null, null, null, null, DisplayName = "Null text returns null")]
        [DataRow("", "tag", "attr", 
            null, null, "", null, DisplayName = "Empty text returns empty before text")]
        [DataRow("No tags here", "tag", "attr", 
            null, null, "No tags here", null, DisplayName = "No matching tag returns original text")]
        [DataRow("Before <paramref name=\"value\"/> After", "paramref", "name", 
            "value", "", "Before ", " After", DisplayName = "Finds short self-closing tag")]
        [DataRow("Before <code lang=\"csharp\">test</code> After", "code", "lang", 
            "csharp", "test", "Before ", " After", DisplayName = "Finds long tag with attribute")]
        [DataRow("Before <see cref=\"Type\"/> Middle <code>test</code> After", "see", "cref", 
            "Type", "", "Before ", " Middle <code>test</code> After", DisplayName = "Prioritizes short tag when both present")]
        [DataRow("Before <see cref=\"Type\"/> After", "see", "href", 
            null, null, "Before <see cref=\"Type\"/> After", null, DisplayName = "Wrong attribute name returns null")]
        [DataRow("Before <paramref name=\"p1\"/> <paramref name=\"p2\"/> After", "paramref", "name", 
            "p1", "", "Before ", " <paramref name=\"p2\"/> After", DisplayName = "Returns first short tag")]
        [DataRow("Before <code lang=\"cs\">var x;</code> <code>y</code> After", "code", "lang", 
            "cs", "var x;", "Before ", " <code>y</code> After", DisplayName = "Returns first long tag with attribute")]
        [DataRow("Text <tag attr=\"val\"/> more <tag attr=\"val2\">content</tag> end", "tag", "attr", 
            "val", "", "Text ", " more <tag attr=\"val2\">content</tag> end", DisplayName = "Short tag takes priority over long tag")]
        public void FindTagWithAttribute(string text, string tagName, string attributeName,
            string expectedAttributeValue, string expectedInnerText, string expectedBeforeText, string expectedAfterText)
        {
            var (attributeValue, innerText, beforeText, afterText) = text.FindTagWithAttribute(tagName, attributeName);

            Assert.AreEqual(expectedAttributeValue, attributeValue);
            Assert.AreEqual(expectedInnerText, innerText);
            Assert.AreEqual(expectedBeforeText, beforeText);
            Assert.AreEqual(expectedAfterText, afterText);
        }
    }
}
