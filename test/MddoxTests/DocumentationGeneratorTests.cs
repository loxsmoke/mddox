using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox;
using MdDox.Localization;
using MdDox.MarkdownFormatters;
using MdDox.Reflection;
using MddoxTests.TestData;
using LoxSmoke.DocXml.Reflection;
using System.IO;

namespace MddoxTests
{
    [TestClass]
    public class DocumentationGeneratorTests
    {
        [DataTestMethod]
        [DataRow(typeof(TestClass), "TestClass Class")]
        [DataRow(typeof(TestStruct), "TestStruct Struct")]
        [DataRow(typeof(TestEnum), "TestEnum Enum")]
        [DataRow(typeof(TestInterface), "TestInterface Interface")]
        [DataRow(typeof(TestRecord), "TestRecord Record")]
        public void TypeTitle_WithVariousTypes_ReturnsCorrectTitle(System.Type type, string expectedTitle)
        {
            var localizedStrings = new LocalizedStrings();
            
            var result = DocumentationGenerator.TypeTitle(type, localizedStrings);
            
            Assert.AreEqual(expectedTitle, result);
        }

        [DataTestMethod]
        [DataRow(typeof(string), "net-8.0", "en-us", "https://docs.microsoft.com/en-us/dotnet/api/system.string?view=net-8.0")]
        [DataRow(typeof(string), "", "en-us", "https://docs.microsoft.com/en-us/dotnet/api/system.string")]
        [DataRow(typeof(string), null, "en-us", "https://docs.microsoft.com/en-us/dotnet/api/system.string")]
        [DataRow(typeof(System.Collections.Generic.List<>), "net-8.0", "en-us", "https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-8.0")]
        [DataRow(typeof(string), "net-8.0", "zh-CN", "https://docs.microsoft.com/zh-cn/dotnet/api/system.string?view=net-8.0")]
        [DataRow(typeof(int), "net-8.0", "en-us", "https://docs.microsoft.com/en-us/dotnet/api/system.int32?view=net-8.0")]
        public void MsdnUrlForType_ReturnsCorrectUrl(System.Type type, string view, string locale, string expectedUrl)
        {
            var result = DocumentationGenerator.MsdnUrlForType(type, view, locale);

            Assert.AreEqual(expectedUrl, result);
        }

        [TestMethod]
        public void BuildDocument_WithAllTestTypes_GeneratesCompleteDocumentation()
        {
            var testAssembly = typeof(TestClass).Assembly;
            var typeCollection = TypeCollection.ForReferencedTypes(testAssembly);
            
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = "Test Documentation",
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                DocumentMethodDetails = true,
                TypeIndexColumnCount = 2,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var result = generator.FullText;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0, "Result should not be empty");
            Assert.IsTrue(result.Contains("Test Documentation"), "Should contain document title");
            Assert.IsTrue(result.Contains("TestClass"), "Should contain TestClass");
            Assert.IsTrue(result.Contains("TestStruct"), "Should contain TestStruct");
            Assert.IsTrue(result.Contains("TestEnum"), "Should contain TestEnum");
            Assert.IsTrue(result.Contains("TestInterface"), "Should contain TestInterface");
            Assert.IsTrue(result.Contains("TestRecord"), "Should contain TestRecord");
            Assert.IsTrue(result.Contains("TestProperty") || result.Contains("Properties"), "Should contain TestProperty or Properties section");
            Assert.IsTrue(result.Contains("TestMethod") || result.Contains("Methods"), "Should contain TestMethod or Methods section");
            
            File.WriteAllText("test_output.md", result);
        }

        [TestMethod]
        public void BuildDocument_WithTestClass_ContainsExpectedSections()
        {
            var typeCollection = TypeCollection.ForReferencedTypes(typeof(TestClass));
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = "TestClass Documentation",
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                DocumentMethodDetails = true,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var result = generator.FullText;

            Assert.IsTrue(result.Contains("TestClass Documentation"));
            Assert.IsTrue(result.Contains("TestClass Class"));
            Assert.IsTrue(result.Contains("Namespace: MddoxTests.TestData"));
            Assert.IsTrue(result.Contains("A test class for documentation generation testing"));
            Assert.IsTrue(result.Contains("## Properties"));
            Assert.IsTrue(result.Contains("TestProperty"));
            Assert.IsTrue(result.Contains("## Methods"));
            Assert.IsTrue(result.Contains("TestMethod"));
        }

        [TestMethod]
        public void BuildDocument_WithTestEnum_ContainsEnumValues()
        {
            var typeCollection = TypeCollection.ForReferencedTypes(typeof(TestEnum));
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = null,
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var result = generator.FullText;

            Assert.IsTrue(result.Contains("TestEnum Enum"));
            Assert.IsTrue(result.Contains("## Values"));
            Assert.IsTrue(result.Contains("First"));
            Assert.IsTrue(result.Contains("Second"));
            Assert.IsTrue(result.Contains("Third"));
            Assert.IsTrue(result.Contains("The first value"));
            Assert.IsTrue(result.Contains("The second value"));
            Assert.IsTrue(result.Contains("The third value"));
        }

        [TestMethod]
        public void BuildDocument_WithMethodDetails_IncludesParameterDetails()
        {
            var typeCollection = TypeCollection.ForReferencedTypes(typeof(TestClass));
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = null,
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                DocumentMethodDetails = true,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var result = generator.FullText;

            Assert.IsTrue(result.Contains("### TestMethod"));
            Assert.IsTrue(result.Contains("input"));
            Assert.IsTrue(result.Contains("The input parameter"));
            Assert.IsTrue(result.Contains("### Returns"));
            Assert.IsTrue(result.Contains("A test string result"));
        }

        [TestMethod]
        public void BuildDocument_WithoutMethodDetails_DoesNotIncludeParameterDetails()
        {
            var typeCollection = TypeCollection.ForReferencedTypes(typeof(TestClass));
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = null,
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                DocumentMethodDetails = false,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var result = generator.FullText;

            Assert.IsFalse(result.Contains("### TestMethod"));
            Assert.IsFalse(result.Contains("### Returns"));
        }

        [TestMethod]
        public void BuildDocument_TestClassDocumentation_MatchesExpectedMarkdown()
        {
            var typeCollection = TypeCollection.ForReferencedTypes(typeof(TestClass));
            var orderedTypeList = new OrderedTypeList(typeCollection);
            var options = new DocumentationGeneratorOptions
            {
                DocumentTitle = "TestClass Documentation",
                ShowDocumentDateTime = false,
                ShowCommandLine = false,
                AddMsdnLinks = false,
                DocumentMethodDetails = true,
                TypeIndexColumnCount = 3,
                Strings = new LocalizedStrings()
            };
            var markdownFormatter = new GithubMarkdownFormatter();
            var generator = new DocumentationGenerator(orderedTypeList, options, markdownFormatter);

            generator.BuildDocument();
            var actualOutput = generator.FullText;

            var expectedFilePath = Path.Combine("TestData", "ExpectedTestClassDocumentation.md");
            Assert.IsTrue(File.Exists(expectedFilePath), $"Expected file not found: {expectedFilePath}");

            var expectedOutput = File.ReadAllText(expectedFilePath);

            // Normalize line endings to make test OS-agnostic
            var normalizedExpected = expectedOutput.Replace("\r\n", "\n").Replace("\r", "\n");
            var normalizedActual = actualOutput.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.AreEqual(normalizedExpected, normalizedActual, 
                "Generated documentation does not match expected output. " +
                "If the change is intentional, update ExpectedTestClassDocumentation.md");
        }
    }
}
