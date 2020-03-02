using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MdDox.MarkdownWriters;
using MdDox.MarkdownWriters.Interfaces;
using DocXml.Reflection;
using LoxSmoke.DocXml;
using LoxSmoke.DocXml.Reflection;

namespace MdDox
{
    /// <summary>
    /// <typeparamref name=""/>
    /// <paramref name=""/>
    /// <![CDATA[]]>
    /// <c></c>
    /// <code></code>
    /// <example></example>
    /// <exception cref=""></exception>
    /// <list type=""></list>
    /// <para></para>
    /// <see cref=""/>
    /// <seealso cref=""/>
    /// </summary>

    class Program
    {
        static void Help()
        {
            Console.WriteLine($"mddox, version {Assembly.GetExecutingAssembly().GetName().Version}, (c) 2019-2020 loxsmoke ");
            Console.WriteLine("Markdown documentation generator. See https://github.com/loxsmoke/mddox for more info.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("   mddox <assembly> <optional-parameters>");
            Console.WriteLine("   <assembly>             The name of the assembly to document.");
            Console.WriteLine("");
            Console.WriteLine("Optional parameters:");
            Console.WriteLine("   -o | --output <output_md>   The name of the markdown output file.");
            Console.WriteLine($"   -f | --format <format>      The markdown file format. Valid values: {MarkdownFormatNames}.");
            Console.WriteLine("   -r | --recursive            Step into referenced assemblies recursively.");
            Console.WriteLine("   -r | --recursive <assembly> Step recursivelly only into specified assembly or assemblies.");
            Console.WriteLine("                               This parameter can be used multiple times to specify multiple assemblies.");
            Console.WriteLine("   -m | --ignore-methods       Do not generate documentation for methods and constructors.");
            Console.WriteLine("   -a | --ignore-attribute <name> ");
            Console.WriteLine("                               Do not generate documentation for properties with specified custom attribute.");
            Console.WriteLine("                               For example JsonIgnoreAttribute");
            Console.WriteLine("                               This parameter can be used multiple times to specify multiple sttributes.");
            Console.WriteLine("   -t | --type <name>          Document only this type and all types referenced by this type.");
            Console.WriteLine("   -d | --msdn [<view>]        Generate links to the MSDN documentation for System.* and Microsoft.* types.");
            Console.WriteLine("                               The documentation pages are located at this site https://docs.microsoft.com");
            Console.WriteLine("                               View is an optional parameter of URL specifying the version of the type. For example: netcore-3.1");
            Console.WriteLine("   -n | --no-title             Do not write the \"created by mddox at date\" in the markdown file.");
            Console.WriteLine("   -v | --verbose              Print some debug info when generating documentation.");
        }

        static CommandLineOptions Parse(string [] args)
        {
            var options = new CommandLineOptions()
            {
                Format = MarkdownWriters.First().FormatName,
            };
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--output":
                    case "-o":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing output file name");
                            return null;
                        }
                        options.OutputFile = args[i];
                        break;
                    case "--format":
                    case "-f":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing format");
                            return null;
                        }
                        options.Format = args[i];
                        break;
                    case "--ignore-methods":
                    case "-m":
                        options.IgnoreMethods = true;
                        break;
                    case "--ignore-attribute":
                    case "-a":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing attribute name");
                            return null;
                        }
                        options.IgnoreAttributes.Add(args[i]);
                        break;
                    case "--recursive":
                    case "-r":
                        options.Recursive = true;
                        if (i + 1 < args.Length &&
                            !args[i + 1].StartsWith('-'))
                        {
                            options.RecursiveAssemblies.Add(args[++i]);
                        }
                        break;
                    case "--type":
                    case "-t":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing type name");
                            return null;
                        }
                        options.TypeName = args[i];
                        break;
                    case "--help":
                    case "-h":
                        return null;
                    case "--msdn":
                    case "-s":
                        options.MsdnLinks = true;
                        if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                        {
                            options.MsdnView = args[++i];
                        }
                        break;
                    case "--no-title":
                    case "-n":
                        options.ShowTitle = false;
                        break;
                    case "--verbose":
                    case "-v":
                        options.Verbose = true;
                        break;
                    default:
                        if (args[i].StartsWith("-"))
                        {
                            Console.WriteLine($"Error: Unknown parameter {args[i]}");
                            return null;
                        }
                        if (options.AssemblyName == null) options.AssemblyName = args[i];
                        break;
                }
            }

            if (options.AssemblyName != null && options.OutputFile == null)
            {
                options.OutputFile = Path.GetFileNameWithoutExtension(options.AssemblyName) + ".md";
            }

            return options;
        }

        static List<IMarkdownWriter> MarkdownWriters = new List<IMarkdownWriter>()
        {
            new GithubMarkdownWriter(),
            new BitbucketMarkdownWriter()
        };
        static string MarkdownFormatNames => string.Join(",", MarkdownWriters.Select(md => md.FormatName));

        static void Main(string[] args)
        {
            var options = Parse(args);
            if (options?.AssemblyName == null)
            {
                Help();
                return;
            }

            var writer = MarkdownWriters.FirstOrDefault(md => md.FormatName.Equals(options.Format, StringComparison.OrdinalIgnoreCase));

            if (options.Format == null)
            {
                writer = MarkdownWriters.First();
                Console.WriteLine($"Markdown format not specified. Assuming {writer.FormatName}.");
            }
            if (writer == null)
            {
                Console.WriteLine($"Error: invalid markdown format specified. Valid values: {MarkdownFormatNames}");
                return;
            }

            try
            {
                var myAssembly = Assembly.LoadFrom(options.AssemblyName);
                if (myAssembly == null)
                {
                    throw new Exception($"Could not load assembly \'{options.AssemblyName}\'");
                }

                Type rootType = null;
                if (options.TypeName != null)
                {
                    rootType = myAssembly.DefinedTypes.FirstOrDefault(t => t.Name == options.TypeName);
                    if (rootType == null)
                    {
                        var possibleTypes = myAssembly.DefinedTypes
                            .Where(t => t.Name.Contains(options.TypeName, StringComparison.OrdinalIgnoreCase))
                            .Select(t => t.Name).ToList();
                        if (possibleTypes.Count == 0)
                            throw new Exception(
                                $"Specified type name \'{options.TypeName}\' not found in assembly \'{options.AssemblyName}\'");

                        throw new Exception(
                            $"Specified type name \'{options.TypeName}\' not found in assembly \'{options.AssemblyName}\'." +
                            $" Similar type names in the assembly: {string.Join(",", possibleTypes)}");
                    }
                }
                DocumentationGenerator.GenerateMarkdown(
                    rootType, 
                    rootType == null ? myAssembly : null, 
                    options.Recursive, 
                    options.RecursiveAssemblies, 
                    options.IgnoreAttributes, 
                    options.IgnoreMethods, 
                    options.MsdnLinks, options.MsdnView, 
                    options.ShowTitle,
                    options.Verbose,
                    writer, 
                    options.OutputFile);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"{exc.StackTrace}");
            }
        }
    }
}
