using CommandLine;
using System;
using System.Collections.Generic;

namespace MdDox.CommandLineOptions
{
    /// <summary>
    /// Mddox command line definition
    /// </summary>
    public class CommandLineOptions
    {
        [Value(0, MetaName = "assembly", Required = true, HelpText = "The name of the assembly to document.")]
        public string AssemblyName { get; set; }
        [Option('o', "output", Required = false, HelpText = "Specify the name of the markdown output file.")]
        public string OutputFile { get; set; }
        [Option('f', "format", Required = false, HelpText = "Specify the markdown file format. Valid values: github,bitbucket.")]
        public string Format { get; set; }
        [Option("all-recursive", Required = false, HelpText = "Step into all referenced assemblies recursively.")]
        public bool AllRecursive { get; set; }
        [Option('r', "recursive", Required = false, HelpText = "Step into specified referenced assemblies recursively.\nSpecify one or more assembly names separated by spaces.")]
        public IEnumerable<string> RecursiveAssemblies { get; set; }
        [Option('m', "ignore-methods", Required = false, HelpText = "Do not generate documentation for methods and constructors.\nUseful for POCO documentation.")]
        public bool IgnoreMethods { get; set; }
        [Option('d', "method-details", Required = false, HelpText = "Generate detailed documentation for methods and constructors.\nSetting has no effect if --ignore-methods is specified.")]
        public bool DocumentMethodDetails { get; set; }
        [Option('a', "ignore-attribute", Required = false, HelpText = "Do not generate documentation for properties with specified custom attribute(s).\nFor example JsonIgnoreAttribute\nMore than one space-separate attribute can be specified.")]
        public IEnumerable<string> IgnoreAttributes { get; set; }
        [Option('t', "type", Required = false, HelpText = "Document only the specified type and all types referenced by it.")]
        public string TypeName { get; set; }
        [Option('s', "msdn", Required = false, HelpText = "Generate links to the specified version of MSDN documentation for System.* and Microsoft.* types.\nThe documentation pages are located at this site https://docs.microsoft.com\nPossible versions can be latest, netcore-3.1, or net-5.0")]
        public string MsdnLinkViewParameter { get; set; }
        [Option('i', "title", Required = false, HelpText = "Document title string format. Use {assembly} and {version} in the format string to\ninsert the name of the assembly and assembly version.")]
        public string DocumentTitle { get; set; }
        [Option('n', "no-title", Required = false, HelpText = "Do not write the \"created by mddox at date\" in the markdown file.")]
        public bool DoNotShowDocumentDateTime { get; set; }
        [Option('v', "verbose", Required = false, HelpText = "Print some debug info when generating documentation.")]
        public bool Verbose { get; set; }
    }
}
