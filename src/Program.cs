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
            Console.WriteLine($"mddox, version {Assembly.GetExecutingAssembly().GetName().Version}, (c) 2019 loxsmoke ");
            Console.WriteLine("Markdown documentation generator. See https://github.com/loxsmoke/mddox for more info.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("   mddox <assembly> <optional-parameters>");
            Console.WriteLine("   <assembly>             The name of the assembly to document.");
            Console.WriteLine("");
            Console.WriteLine("Optional parameters:");
            Console.WriteLine("   --output <output_md>   The name of the markdown output file.");
            Console.WriteLine($"   --format <format>      The markdown file format. Valid values: {MarkdownFormatNames}.");
            Console.WriteLine("   --recursive            Step into referenced assemblies recursively.");
            Console.WriteLine("   --recursive <assembly> Step recursivelly only into specified assembly or assemblies.");
            Console.WriteLine("   --ignore-methods       Do not generate documentation for methods and constructors.");
            Console.WriteLine("   --ignore-attribute <name> " + Environment.NewLine +
                              "                          Do not generate documentation for properties with specified custom attribute." + Environment.NewLine +
                              "                          For example JsonIgnoreAttribute");
            Console.WriteLine("   --type <name>          Document specified only this and referenced types.");
        }

        static (string AssemblyName, string TypeName,  string OutputFile, string Format, bool IgnoreMethods, List<string> IgnoreAttributes, 
            bool Recursive, List<string> RecursiveAssemblies) Parse(string [] args)
        {
            string assemblyName = null, typeName = null, outputFile = null, format = MarkdownWriters.First().FormatName;
            var ignoreMethods = false;
            var ignoreAttributes = new List<string>();
            var recursive = false;
            var recursiveAssemblies = new List<string>();
            var errorReturn = ((string)null, (string)null, (string)null, (string)null, false, (List<string>)null, false, (List<string>)null);
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--output":
                    case "-o":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing output file name");
                            return errorReturn;
                        }
                        outputFile = args[i];
                        break;
                    case "--format":
                    case "-f":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing format");
                            return errorReturn;
                        }
                        format = args[i];
                        break;
                    case "--ignore-methods":
                    case "-m":
                        ignoreMethods = true;
                        break;
                    case "--ignore-attribute":
                    case "-a":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing attribute name");
                            return errorReturn;
                        }
                        ignoreAttributes.Add(args[i]);
                        break;
                    case "--recursive":
                    case "-r":
                        recursive = true;
                        if (i + 1 < args.Length &&
                            !args[i + 1].StartsWith('-'))
                        {
                            recursiveAssemblies.Add(args[++i]);
                        }
                        break;
                    case "--type":
                    case "-t":
                        if (++i == args.Length)
                        {
                            Console.WriteLine("Error: Missing type name");
                            return errorReturn;
                        }
                        typeName = args[i];
                        break;
                    case "--help":
                    case "-h":
                        return errorReturn;
                    default:
                        if (args[i].StartsWith("-"))
                        {
                            Console.WriteLine($"Error: Unknown parameter {args[i]}");
                            return errorReturn;
                        }
                        if (assemblyName == null) assemblyName = args[i];
                        break;
                }
            }

            if (assemblyName != null && outputFile == null)
            {
                outputFile = Path.GetFileNameWithoutExtension(assemblyName) + ".md";
            }

            return (assemblyName, typeName, outputFile, format, ignoreMethods, ignoreAttributes, recursive, recursiveAssemblies);
        }

        static List<IMarkdownWriter> MarkdownWriters = new List<IMarkdownWriter>()
        {
            new GithubMarkdownWriter(),
            new BitbucketMarkdownWriter()
        };
        static string MarkdownFormatNames => string.Join(",", MarkdownWriters.Select(md => md.FormatName));

        static void Main(string[] args)
        {
            var (assemblyName, typeName, outputFile, format, ignoreMethods, ignoreAttributes, recursive, recursiveAssemblies) = Parse(args);
            if (assemblyName == null)
            {
                Help();
                return;
            }

            var writer = MarkdownWriters.FirstOrDefault(md => md.FormatName.Equals(format, StringComparison.OrdinalIgnoreCase));

            if (format == null)
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
                var myAssembly = Assembly.LoadFrom(assemblyName);
                if (myAssembly == null)
                {
                    throw new Exception($"Could not load assembly \'{assemblyName}\'");
                }

                Type rootType = null;
                if (typeName != null)
                {
                    rootType = myAssembly.DefinedTypes.FirstOrDefault(t => t.Name == typeName);
                    if (rootType == null)
                    {
                        var possibleTypes = myAssembly.DefinedTypes
                            .Where(t => t.Name.Contains(typeName, StringComparison.OrdinalIgnoreCase))
                            .Select(t => t.Name).ToList();
                        if (possibleTypes.Count == 0)
                            throw new Exception(
                                $"Specified type name \'{typeName}\' not found in assembly \'{assemblyName}\'");

                        throw new Exception(
                            $"Specified type name \'{typeName}\' not found in assembly \'{assemblyName}\'." +
                            $" Similar type names in the assembly: {string.Join(",", possibleTypes)}");
                    }
                }
                DocumentationGenerator.GenerateMarkdown(rootType, 
                    rootType == null ? myAssembly : null, recursive, recursiveAssemblies, ignoreAttributes, ignoreMethods, writer, outputFile);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"{exc.StackTrace}");
            }
        }
    }
}
