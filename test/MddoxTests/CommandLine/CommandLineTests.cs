using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using CommandLine;

namespace MddoxTests.CommandLine
{
    public class CommandLineTests
    {
        [Theory()]
        [InlineData("assembly", "-o", "output", "-f","github")]
        [InlineData("assembly", "--output", "output", "--format", "github", "--all-recursive")]
        [InlineData("assembly", "-r", "assembly1", "assembly2")]
        [InlineData("assembly", "--recursive", "assembly1", "assembly2")]
        [InlineData("assembly", "--include", "all.all", "all.public")]
        [InlineData("assembly", "--exclude", "all.all", "all.public")]
        [InlineData("assembly", "-m", "-d", "-n", "-v")]
        [InlineData("assembly", "--ignore-methods", "--method-details", "--no-title", "--verbose")]
        [InlineData("assembly", "-a", "Attribute1", "Attribute2")]
        [InlineData("assembly", "--ignore-attribute", "Attribute1", "Attribute2")]
        [InlineData("assembly", "-t", "Dictionary")]
        [InlineData("assembly", "--type", "Dictionary")]
        [InlineData("assembly", "-s", "latest")]
        [InlineData("assembly", "--msdn", "latest")]
        [InlineData("assembly", "-i", "{assembly} and {version}")]
        [InlineData("assembly", "--title", "{assembly} and {version}")]
        public void Parse_Valid(params string[] args)
        {
            var result = MdDox.CommandLineOptions.CliProgram.Parse(args);
            Assert.Equal(ParserResultType.Parsed, result.Tag);
        }
    }
}
