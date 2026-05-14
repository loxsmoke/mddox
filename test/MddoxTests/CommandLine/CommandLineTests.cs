using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLine;

namespace MddoxTests.CommandLine
{
    [TestClass]
    public class CommandLineTests
    {
        [DataTestMethod]
        [DataRow("assembly", "-o", "output", "-f","github")]
        [DataRow("assembly", "--output", "output", "--format", "github", "--all-recursive")]
        [DataRow("assembly", "-r", "assembly1", "assembly2")]
        [DataRow("assembly", "--recursive", "assembly1", "assembly2")]
        [DataRow("assembly", "--include", "all.all", "all.public")]
        [DataRow("assembly", "--exclude", "all.all", "all.public")]
        [DataRow("assembly", "-m", "-d", "-n", "-v")]
        [DataRow("assembly", "--ignore-methods", "--method-details", "--no-title", "--verbose")]
        [DataRow("assembly", "-a", "Attribute1", "Attribute2")]
        [DataRow("assembly", "--ignore-attribute", "Attribute1", "Attribute2")]
        [DataRow("assembly", "-t", "Dictionary")]
        [DataRow("assembly", "--type", "Dictionary")]
        [DataRow("assembly", "-s", "latest")]
        [DataRow("assembly", "--msdn", "latest")]
        [DataRow("assembly", "-i", "{assembly} and {version}")]
        [DataRow("assembly", "--title", "{assembly} and {version}")]
        public void Parse_Valid(params string[] args)
        {
            var result = MdDox.CommandLineOptions.CliProgram.Parse(args);
            Assert.AreEqual(ParserResultType.Parsed, result.Tag);
        }
    }
}
