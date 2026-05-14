using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox.MarkdownFormatters;
using System;

namespace MddoxTests.MarkdownFormatters
{
    /// <summary>
    /// Tests for BitbucketMarkdownFormatter.
    /// Only tests methods that are specifically implemented in BitbucketMarkdownFormatter.
    /// </summary>
    [TestClass]
    public class BitbucketMarkdownFormatterTests
    {
        private BitbucketMarkdownFormatter _formatter;

        [TestInitialize]
        public void Setup()
        {
            _formatter = new BitbucketMarkdownFormatter();
        }

        [TestMethod]
        public void Name_ReturnsBitbucket()
        {
            var result = _formatter.Name;
            Assert.AreEqual("bitbucket", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ReturnsEmptyString_WhenInputIsNull()
        {
            var result = _formatter.EscapeSpecialChars(null);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_DoesNotEscapeLessThanSign()
        {
            var result = _formatter.EscapeSpecialChars("Text<More");

            // Bitbucket doesn't escape < like Github/Azure
            Assert.AreEqual("Text<More", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_DoesNotEscapeGreaterThanSign()
        {
            var result = _formatter.EscapeSpecialChars("Text>More");

            // Bitbucket doesn't escape > like Github/Azure
            Assert.AreEqual("Text>More", result);
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
            Assert.AreEqual("Value>10", result);
        }

        [TestMethod]
        public void EscapeSpecialChars_ConvertsHtmlEntityLessThan()
        {
            var input = "Value&lt;10";
            var result = _formatter.EscapeSpecialChars(input);
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

            // Bitbucket doesn't escape < and > after conversion
            Assert.AreEqual("List<string><br>values\\|more<br>end", result);
        }

        [TestMethod]
        public void HeadingLink_AddsMarkdownHeaderPrefix()
        {
            var anchorName = "my-section";
            var result = _formatter.HeadingLink(anchorName);
            Assert.AreEqual("[my-section](#markdown-header-my-section)", result);
        }

        [TestMethod]
        public void HeadingLink_UsesCustomTextWhenProvided()
        {
            var anchorName = "section-id";
            var text = "Click Here";
            var result = _formatter.HeadingLink(anchorName, text);
            Assert.AreEqual("[Click Here](#markdown-header-section-id)", result);
        }

        [TestMethod]
        public void HeadingLink_CleansUpAnchorName()
        {
            var anchorName = "Section With Spaces";
            var result = _formatter.HeadingLink(anchorName);

            // Should clean up and add markdown-header- prefix
            Assert.IsTrue(result.Contains("#markdown-header-"));
            Assert.IsTrue(result.Contains("section-with-spaces"));
        }

        [TestMethod]
        public void Heading_AddsNewlineBeforeHeading()
        {
            var result = _formatter.Heading(1, "Title");

            // Bitbucket adds an extra newline at the beginning
            Assert.IsTrue(result.StartsWith(Environment.NewLine));
        }

        [TestMethod]
        public void Heading_CallsBaseHeadingImplementation()
        {
            var result = _formatter.Heading(2, "Subtitle");

            // Should have newline prefix + base implementation (## + text + double newline)
            Assert.IsTrue(result.StartsWith(Environment.NewLine + "## "));
            Assert.IsTrue(result.EndsWith(Environment.NewLine + Environment.NewLine));
        }

        [DataTestMethod]
        [DataRow(1, "H1")]
        [DataRow(2, "H2")]
        [DataRow(3, "H3")]
        [DataRow(4, "H4")]
        [DataRow(5, "H5")]
        [DataRow(6, "H6")]
        public void Heading_WorksForAllLevels(int level, string text)
        {
            var result = _formatter.Heading(level, text);
            Assert.IsTrue(result.StartsWith(Environment.NewLine));
            Assert.IsTrue(result.Contains(new string('#', level) + " "));
        }
    }
}
