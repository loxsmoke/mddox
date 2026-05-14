using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox.MarkdownFormatters;
using System;

namespace MddoxTests.MarkdownFormatters
{
    /// <summary>
    /// Tests for the base MarkdownFormatter class using GithubMarkdownFormatter as the concrete implementation.
    /// These tests verify all non-abstract methods from the MarkdownFormatter base class.
    /// </summary>
    [TestClass]
    public class MarkdownFormatterTests
    {
        private GithubMarkdownFormatter _formatter;

        [TestInitialize]
        public void Setup()
        {
            _formatter = new GithubMarkdownFormatter();
        }

        [TestMethod]
        public void NewLine_ReturnsHtmlBreak()
        {
            var result = _formatter.NewLine;
            Assert.AreEqual("<br>", result);
        }

        [TestMethod]
        public void StartInlineCode_ReturnsBacktick()
        {
            var result = _formatter.StartInlineCode;
            Assert.AreEqual("`", result);
        }

        [TestMethod]
        public void EndInlineCode_ReturnsBacktick()
        {
            var result = _formatter.EndInlineCode;
            Assert.AreEqual("`", result);
        }

        [TestMethod]
        public void StartMultilineCode_ReturnsTripleBackticksWithNewline()
        {
            var result = _formatter.StartMultilineCode;
            Assert.AreEqual("```" + Environment.NewLine, result);
        }

        [TestMethod]
        public void EndMultilineCode_ReturnsNewlineWithTripleBackticksAndNewline()
        {
            var result = _formatter.EndMultilineCode;
            Assert.AreEqual(Environment.NewLine + "```" + Environment.NewLine, result);
        }

        [TestMethod]
        public void Bold_WrapTextWithDoubleAsterisks()
        {
            var text = "bold text";
            var result = _formatter.Bold(text);
            Assert.AreEqual("**bold text**", result);
        }

        [DataTestMethod]
        [DataRow(1, "Title", "# Title")]
        [DataRow(2, "Subtitle", "## Subtitle")]
        [DataRow(3, "Section", "### Section")]
        [DataRow(4, "Subsection", "#### Subsection")]
        [DataRow(5, "Subsubsection", "##### Subsubsection")]
        [DataRow(6, "Paragraph", "###### Paragraph")]
        public void Heading_CreatesCorrectMarkdownHeading(int level, string text, string expectedPrefix)
        {
            var result = _formatter.Heading(level, text);
            Assert.IsTrue(result.StartsWith(expectedPrefix));
            Assert.IsTrue(result.EndsWith(Environment.NewLine + Environment.NewLine));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(7)]
        [DataRow(10)]
        public void Heading_ThrowsArgumentOutOfRangeException_ForInvalidLevel(int invalidLevel)
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _formatter.Heading(invalidLevel, "text"));
        }

        [TestMethod]
        public void Heading_EscapesSpecialCharacters()
        {
            var textWithSpecialChars = "Title<Test>";
            var result = _formatter.Heading(1, textWithSpecialChars);
            Assert.IsTrue(result.Contains("\\<"));
            Assert.IsTrue(result.Contains("\\>"));
        }

        [TestMethod]
        public void HeadingLink_CreatesLinkWithAnchorName()
        {
            var anchorName = "my-section";
            var result = _formatter.HeadingLink(anchorName);
            Assert.AreEqual("[my-section](#my-section)", result);
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
        public void HeadingLink_CleansUpAnchorName()
        {
            var anchorName = "Section With Spaces";
            var result = _formatter.HeadingLink(anchorName);

            // The CleanupHeadingAnchor should convert spaces to hyphens and lowercase
            Assert.IsTrue(result.Contains("section-with-spaces"));
        }

        [TestMethod]
        public void Link_CreatesMarkdownLink()
        {
            var url = "https://example.com";
            var text = "Example Site";
            var result = _formatter.Link(url, text);
            Assert.AreEqual("[Example Site](https://example.com)", result);
        }

        [TestMethod]
        public void TableRow_CreatesFormattedTableRow()
        {
            var cells = new[] { "Cell1", "Cell2", "Cell3" };
            var result = _formatter.TableRow(cells);
            Assert.AreEqual("| Cell1 | Cell2 | Cell3 |" + Environment.NewLine, result);
        }

        [TestMethod]
        public void TableRow_HandlesEmptyArray()
        {
            var result = _formatter.TableRow();
            Assert.AreEqual("|  |" + Environment.NewLine, result);
        }

        [TestMethod]
        public void TableRow_HandlesSingleCell()
        {
            var result = _formatter.TableRow("OnlyCell");
            Assert.AreEqual("| OnlyCell |" + Environment.NewLine, result);
        }

        [TestMethod]
        public void TableTitle_CreatesTableHeaderWithSeparator()
        {
            var headings = new[] { "Column1", "Column2", "Column3" };
            var result = _formatter.TableTitle(headings);
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("| Column1 | Column2 | Column3 |", lines[0]);
            Assert.AreEqual("|---|---|---|", lines[1]);
        }

        [TestMethod]
        public void TableTitle_HandlesEmptyArray()
        {
            var result = _formatter.TableTitle();
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("|  |", lines[0]);
            Assert.AreEqual("||", lines[1]);
        }

        [TestMethod]
        public void WithNewline_AppendsDoubleNewline()
        {
            var text = "Some text";
            var result = _formatter.WithNewline(text);
            Assert.AreEqual("Some text" + Environment.NewLine + Environment.NewLine, result);
        }

        [TestMethod]
        public void WithNewline_HandlesEmptyString()
        {
            var result = _formatter.WithNewline("");
            Assert.AreEqual(Environment.NewLine + Environment.NewLine, result);
        }
    }
}
