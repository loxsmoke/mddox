using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox;
using MdDox.Localization;

namespace MddoxTests
{
    [TestClass]
    public class ProgramTests
    {
        [DataTestMethod]
        [DataRow(null, null, DisplayName = "Null assembly and null format returns null")]
        public void GenerateTitle_WithNullAssemblyAndNullFormat_ReturnsNull(string format, string expected)
        {
            var localizedStrings = new LocalizedStrings();

            var result = Program.GenerateTitle(null, format, localizedStrings);

            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("", "", DisplayName = "Null assembly and empty format returns empty")]
        [DataRow("Custom Title", "Custom Title", DisplayName = "Custom format with null assembly")]
        [DataRow("{assembly} Documentation", " Documentation", DisplayName = "Assembly placeholder with null assembly")]
        [DataRow("{version} API", " API", DisplayName = "Version placeholder with null assembly")]
        [DataRow("{assembly} {version}", " ", DisplayName = "Both placeholders with null assembly")]
        public void GenerateTitle_WithNullAssembly_ReplacesPlaceholdersWithEmpty(string format, string expected)
        {
            var localizedStrings = new LocalizedStrings();

            var result = Program.GenerateTitle(null, format, localizedStrings);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GenerateTitle_WithAssemblyAndNullFormat_UsesDefaultFormat()
        {
            var assembly = typeof(ProgramTests).Assembly;
            var localizedStrings = new LocalizedStrings();
            var expectedAssemblyName = System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);
            var expectedVersion = localizedStrings.VersionPrefix + assembly.GetName().Version;
            var expectedTitle = $"{expectedAssemblyName} {expectedVersion} API documentation";

            var result = Program.GenerateTitle(assembly, null, localizedStrings);

            Assert.AreEqual(expectedTitle, result);
        }

        [TestMethod]
        public void GenerateTitle_WithAssemblyAndCustomFormat_ReplacesPlaceholders()
        {
            var assembly = typeof(ProgramTests).Assembly;
            var localizedStrings = new LocalizedStrings();
            var format = "API for {assembly} - {version}";
            var expectedAssemblyName = System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);
            var expectedVersion = localizedStrings.VersionPrefix + assembly.GetName().Version;
            var expectedTitle = $"API for {expectedAssemblyName} - {expectedVersion}";

            var result = Program.GenerateTitle(assembly, format, localizedStrings);

            Assert.AreEqual(expectedTitle, result);
        }

        [DataTestMethod]
        [DataRow("Documentation for {assembly}", DisplayName = "Only assembly placeholder")]
        [DataRow("Version {version} Documentation", DisplayName = "Only version placeholder")]
        [DataRow("{assembly} v{version}", DisplayName = "Both placeholders")]
        [DataRow("Static Title", DisplayName = "No placeholders")]
        [DataRow("{assembly}{assembly}", DisplayName = "Duplicate assembly placeholder")]
        [DataRow("{version}{version}", DisplayName = "Duplicate version placeholder")]
        public void GenerateTitle_WithVariousFormats_ReplacesCorrectly(string format)
        {
            var assembly = typeof(ProgramTests).Assembly;
            var localizedStrings = new LocalizedStrings();
            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);
            var version = localizedStrings.VersionPrefix + assembly.GetName().Version;

            var result = Program.GenerateTitle(assembly, format, localizedStrings);

            var expected = format.Replace("{assembly}", assemblyName).Replace("{version}", version);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("", DisplayName = "Empty format string")]
        [DataRow("   ", DisplayName = "Whitespace format string")]
        public void GenerateTitle_WithEmptyFormat_ReturnsReplacedString(string format)
        {
            var assembly = typeof(ProgramTests).Assembly;
            var localizedStrings = new LocalizedStrings();

            var result = Program.GenerateTitle(assembly, format, localizedStrings);

            Assert.AreEqual(format, result);
        }

        [TestMethod]
        public void GenerateTitle_WithComplexFormat_HandlesMultipleReplacements()
        {
            var assembly = typeof(ProgramTests).Assembly;
            var localizedStrings = new LocalizedStrings();
            var format = "{assembly} API Documentation - {version} - Generated for {assembly}";
            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);
            var version = localizedStrings.VersionPrefix + assembly.GetName().Version;
            var expectedTitle = $"{assemblyName} API Documentation - {version} - Generated for {assemblyName}";

            var result = Program.GenerateTitle(assembly, format, localizedStrings);

            Assert.AreEqual(expectedTitle, result);
        }
    }
}
