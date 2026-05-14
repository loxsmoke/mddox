using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox.MarkdownFormatters;
using System;

namespace MddoxTests.MarkdownFormatters
{
    /// <summary>
    /// Tests for GithubMarkdownFormatter.
    /// Only tests methods that are specifically implemented in GithubMarkdownFormatter.
    /// </summary>
    [TestClass]
    public class GithubMarkdownFormatterTests
    {
        private GithubMarkdownFormatter _formatter;

        [TestInitialize]
        public void Setup()
        {
            _formatter = new GithubMarkdownFormatter();
        }

        [TestMethod]
        public void Name_ReturnsGithub()
        {
            var result = _formatter.Name;
            Assert.AreEqual("github", result);
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
        public void EscapeSpecialChars_ReplacesNewlineCharWithBr()
        {
            var input = "Line1\nLine2";
            var result = _formatter.EscapeSpecialChars(input);
            Assert.AreEqual("Line1<br>Line2", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_HandlesMultipleSpecialCharacters()
        {
            var input = "<tag>value|more&gt;text&lt;end|";
            var result = _formatter.EscapeSpecialChars(input);

            // < and > are escaped first, then &gt; and &lt; are converted to unescaped > and <
            Assert.AreEqual("\\<tag\\>value\\|more>text<end\\|", result);
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
        public void EscapeSpecialChars_HandlesComplexScenario()
        {
            var input = "List&lt;string&gt;" + Environment.NewLine + "values|more\nend";
            var result = _formatter.EscapeSpecialChars(input);

            // &lt; and &gt; are converted to unescaped < and >, pipes escaped, newlines become <br>
            Assert.AreEqual("List<string><br>values\\|more<br>end", result);
        }
    }
}
