using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox.MarkdownFormatters;
using System;

namespace MddoxTests.MarkdownFormatters
{
    /// <summary>
    /// Tests for AzureMarkdownFormatter.
    /// Only tests methods that are specifically implemented in AzureMarkdownFormatter.
    /// </summary>
    [TestClass]
    public class AzureMarkdownFormatterTests
    {
        private AzureMarkdownFormatter _formatter;

        [TestInitialize]
        public void Setup()
        {
            _formatter = new AzureMarkdownFormatter();
        }

        [TestMethod]
        public void Name_ReturnsAzure()
        {
            var result = _formatter.Name;
            Assert.AreEqual("azure", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ReturnsEmptyString_WhenInputIsNull()
        {
            var result = _formatter.EscapeSpecialChars(null);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_EscapesLessThanSign()
        {
            var result = _formatter.EscapeSpecialChars("Text<More");
            Assert.AreEqual("Text\\<More", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_EscapesGreaterThanSign()
        {
            var result = _formatter.EscapeSpecialChars("Text>More");
            Assert.AreEqual("Text\\>More", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_EscapesPipeCharacter()
        {
            var result = _formatter.EscapeSpecialChars("Col1|Col2");
            Assert.AreEqual("Col1\\|Col2", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ConvertsHtmlEntityGreaterThan()
        {
            var input = "Value&gt;10";
            var result = _formatter.EscapeSpecialChars(input);

            // Should convert &gt; to > (unescaped)
            Assert.AreEqual("Value>10", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ConvertsHtmlEntityLessThan()
        {
            var input = "Value&lt;10";
            var result = _formatter.EscapeSpecialChars(input);

            // Should convert &lt; to < (unescaped)
            Assert.AreEqual("Value<10", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ReplacesEnvironmentNewLineWithBr()
        {
            var input = "Line1" + Environment.NewLine + "Line2";
            var result = _formatter.EscapeSpecialChars(input);
            Assert.AreEqual("Line1<br>Line2", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_DoesNotReplaceSimpleNewlineChar()
        {
            var input = "Line1\nLine2";
            var result = _formatter.EscapeSpecialChars(input);

            // Azure formatter only replaces Environment.NewLine, not \n
            Assert.AreEqual("Line1\nLine2", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_HandlesEmptyString()
        {
            var result = _formatter.EscapeSpecialChars("");
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_HandlesTextWithoutSpecialChars()
        {
            var input = "Regular text without special characters";
            var result = _formatter.EscapeSpecialChars(input);
            Assert.AreEqual("Regular text without special characters", result);
        }

        [TestMethod]
        public void HeadingLink_CreatesLinkWithLowercaseAnchor()
        {
            var anchorName = "MySection";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("#mysection"));
        }

        [TestMethod]
        public void HeadingLink_UsesCustomTextWhenProvided()
        {
            var anchorName = "section-id";
            var text = "Click Here";
            var result = _formatter.HeadingLink(anchorName, text);
            Assert.AreEqual("[Click Here](#section-id)", result);
        }

        [TestMethod]
        public void HeadingLink_EncodesLessThanSign()
        {
            var anchorName = "List<T>";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("%5C%3C"));
        }

        [TestMethod]
        public void HeadingLink_EncodesGreaterThanSign()
        {
            var anchorName = "List<T>";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("%5C%3E"));
        }

        [TestMethod]
        public void HeadingLink_EncodesComma()
        {
            var anchorName = "Method(int x, int y)";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("%2C"));
        }

        [TestMethod]
        public void HeadingLink_ReplacesSpacesWithHyphens()
        {
            var anchorName = "My Section Title";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("my-section-title"));
        }

        [TestMethod]
        public void HeadingLink_RemovesInvalidCharacters()
        {
            var anchorName = "Section@With#Invalid$Chars!";
            var result = _formatter.HeadingLink(anchorName);

            // Should remove special characters except alphanumeric, (), %, space, and hyphen
            // The result contains one # for the markdown anchor, so check the cleaned part
            var anchorPart = result.Split('#')[1].TrimEnd(')');
            Assert.IsFalse(anchorPart.Contains("@"));
            Assert.IsFalse(anchorPart.Contains("$"));
            Assert.IsFalse(anchorPart.Contains("!"));
        }

        [TestMethod]
        public void HeadingLink_PreservesParentheses()
        {
            var anchorName = "Method(string param)";
            var result = _formatter.HeadingLink(anchorName);
            Assert.IsTrue(result.Contains("("));
            Assert.IsTrue(result.Contains(")"));
        }

        [TestMethod]
        public void HeadingLink_HandlesComplexAnchorName()
        {
            var anchorName = "List<T> Method(int x, string y)";
            var result = _formatter.HeadingLink(anchorName);

            // Should be lowercase, encode < > and comma, replace spaces with hyphens
            Assert.IsTrue(result.Contains("%5C%3C"));
            Assert.IsTrue(result.Contains("%5C%3E"));
            Assert.IsTrue(result.Contains("%2C"));
            Assert.IsTrue(result.Contains("-"));
        }
    }
}
