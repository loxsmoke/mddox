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
using System.ComponentModel.DataAnnotations;

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
            Console.WriteLine($"mddox, version {Assembly.GetExecutingAssembly().GetName().Version}, (c) 2019-2021 loxsmoke ");
            Console.WriteLine("Markdown documentation generator. See https://github.com/loxsmoke/mddox for more info.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("   mddox <assembly> <optional-parameters>");
            Console.WriteLine("   <assembly>             The name of the assembly to document.");
            Console.WriteLine("");
            Console.WriteLine("Optional parameters:");
            Console.WriteLine("   -o | --output <output_md>   The name of the markdown output file.");
            Console.WriteLine($"   -f | --format <format>      The markdown file format. Valid values: {MarkdownFormatNames}.");
            Console.WriteLine("   --all-recursive             Step into referenced assemblies recursively.");
            Console.WriteLine("   -r | --recursive <assembly> Step recursivelly only into specified assembly or assemblies.");
            Console.WriteLine("                               This parameter can be used multiple times to specify multiple assemblies.");
            Console.WriteLine("   -m | --ignore-methods       Do not generate documentation for methods and constructors.");
            Console.WriteLine("   -d | --method-details       Generate detailed documentation for methods and constructors.");
            Console.WriteLine("                               Setting has no effect if --ignore-methods is specified.");
            Console.WriteLine("   -a | --ignore-attribute <name> ");
            Console.WriteLine("                               Do not generate documentation for properties with specified custom attribute.");
            Console.WriteLine("                               For example JsonIgnoreAttribute");
            Console.WriteLine("                               This parameter can be used multiple times to specify multiple sttributes.");
            Console.WriteLine("   -t | --type <name>          Document only this type and all types referenced by this type.");
            Console.WriteLine("   -s | --msdn [<view>]        Generate links to the MSDN documentation for System.* and Microsoft.* types.");
            Console.WriteLine("                               The documentation pages are located at this site https://docs.microsoft.com");
            Console.WriteLine("                               View is an optional parameter of URL specifying the version of the type. For example: netcore-3.1");
            Console.WriteLine("   -i | --title \"title\"        Document title. Use {assembly} and {version} in the format string to");
            Console.WriteLine("                               insert the name of the assembly and assembly version.");
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
                        options.DocumentMethodDetails = false;
                        break;
                    case "--method-details":
                    case "-d":
                        if (!options.IgnoreMethods) options.DocumentMethodDetails = true;
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
                    case "--all-recursive":
                        options.AllRecursive = true;
                        break;
                    case "--recursive":
                    case "-r":
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
                            options.MsdnLinkViewParameter = args[++i];
                        }
                        break;
                    case "--no-title":
                    case "-n":
                        options.ShowDocumentDateTime = false;
                        break;
                    case "--title":
                    case "-i":
                        if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                        {
                            options.DocumentTitle = args[++i];
                        }
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
                if (!File.Exists(options.AssemblyName)) throw new FileNotFoundException("File not found", options.AssemblyName);

                var fullAssemblyName = Path.GetFullPath(options.AssemblyName);
                if (options.Verbose) Console.WriteLine($"Document full assembly file name: \"{fullAssemblyName}\"");

                if (options.Verbose) AppDomain.CurrentDomain.AssemblyLoad += ShowAssemblyLoaded;
                AppDomain.CurrentDomain.AssemblyResolve += 
                    (sender, args) => ResolveAssembly(sender, args, options.Verbose, Path.GetDirectoryName(fullAssemblyName));

                var myAssembly = Assembly.LoadFile(fullAssemblyName);
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
                var recursive = options.AllRecursive || options.RecursiveAssemblies.Count > 0;
                if (options.AllRecursive) options.RecursiveAssemblies.Clear();

                var assembly = rootType == null ? myAssembly : null;
                var typeList = OrderedTypeList.LoadTypes(
                    rootType, 
                    assembly, 
                    recursive, 
                    options.RecursiveAssemblies, 
                    options.IgnoreAttributes, 
                    options.IgnoreMethods, 
                    options.Verbose);

                DocumentationGenerator.GenerateMarkdown(
                    typeList,
                    GenerateTitle(assembly, options.DocumentTitle),
                    options.ShowDocumentDateTime,
                    options.DocumentMethodDetails,
                    options.MsdnLinks, 
                    options.MsdnLinkViewParameter,
                    writer);

                // Write markdown to the output file
                File.WriteAllText(options.OutputFile, writer.FullText);
            }
            catch (BadImageFormatException exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"Hresult:{exc.HResult}");
                if (!exc.HelpLink.IsNullOrEmpty()) Console.WriteLine($"Help link: {exc.HelpLink}");
                Console.WriteLine($"{exc.StackTrace}");
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"{exc.StackTrace}");
            }
        }

        protected static string GenerateTitle(Assembly assembly, string format)
        {
            if (format == null && assembly == null) return null;
            var assemblyName = assembly == null ? "" : Path.GetFileName(assembly.ManifestModule.Name);
            var version = assembly == null ? "" : ("v." + assembly.GetName().Version);
            if (format == null) format = "{assembly} {version} API documentation";
            return format.Replace("{assembly}", assemblyName).Replace("{version}", version);
        }

        private static Dictionary<string, Assembly> RequestedAssemblies { get; set; } = new Dictionary<string, Assembly>();

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args, bool verbose, string basePath)
        {
            var shortAssemblyName = args.Name.Split(',').First();

            // Avoid stack overflow
            if (RequestedAssemblies.ContainsKey(shortAssemblyName)) return RequestedAssemblies[shortAssemblyName];
            RequestedAssemblies.Add(shortAssemblyName, null);
            var fullAssemblyName = Path.GetFullPath(args.Name.Split(',').First() + ".dll", basePath);

            if (verbose)
            {
                Console.WriteLine($"Resolving: {args.Name}");
                if (args.RequestingAssembly != null)
                {
                    Console.WriteLine($"Requested by: {args.RequestingAssembly}");
                    Console.WriteLine($"Requested by: {args.RequestingAssembly.Location}");
                }
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Console.WriteLine("  Already loaded: " + a.FullName);
                }
                Console.WriteLine("Loading: " + fullAssemblyName);
            }
            var assembly = Assembly.LoadFile(fullAssemblyName);
            RequestedAssemblies[shortAssemblyName] = assembly;
            return assembly;
        }

        private static void ShowAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine("Loaded assembly: " + args.LoadedAssembly.FullName);
        }
    }
}
