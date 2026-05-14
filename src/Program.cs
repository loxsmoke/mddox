using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MdDox.MarkdownFormatters;
using MdDox.MarkdownFormatters.Interfaces;
using MdDox.CommandLineOptions;
using MdDox.Reflection;
using System.Globalization;
using MdDox.Localization;
using MdDox.Localization.Interfaces;

namespace MdDox
{
    class Program
    {
        /// <summary>
        /// The list of all markdown formats that mddox currently supports. 
        /// The markdown format can be specified via the command line. 
        /// If not specified, the first format in the list will be used as default.
        /// </summary>
        static readonly List<IMarkdownFormatter> MarkdownWriters = [
            new GithubMarkdownFormatter(),
            new BitbucketMarkdownFormatter(),
            new AzureMarkdownFormatter()
        ];

        /// <summary>
        /// Friendly list of markdown format names for error messages. Generated from the list of markdown writers.
        /// </summary>
        static string MarkdownFormatNames => string.Join(",", MarkdownWriters.Select(md => md.Name));

        /// <summary>
        /// Collection of available localized string providers.
        /// </summary>
        static readonly List<ILocalizedStrings> LocalizedStrings = [
            new LocalizedStrings(), 	// Default en-us
            new LocalizedStringsZhCn()  // zh-cn
        ];

        /// <summary>
        /// Gets a comma-separated string of culture names from the localized strings.
        /// </summary>
        static string LocalizedStringsCultureNames => string.Join(",", LocalizedStrings.Select(md => md.CultureName));

        static void Main(string[] args)
        {
            try
            {
                #region Parse command line and validate options
                var (options, typeFilterOptions) = Parse(args);
                if (options == null)
                {
                    return;
                }

                var writer = MarkdownWriters.FirstOrDefault(md => md.Name.EqualsIgnoreCase(options.Format));
                var localizedStrings = LocalizedStrings.FirstOrDefault(ls => ls.CultureName.EqualsIgnoreCase(options.OutputLanguage))
                    ?? LocalizedStrings.First();
                #endregion

                #region Load assembly and generate type list
                if (!File.Exists(options.AssemblyName)) throw new FileNotFoundException("File not found", options.AssemblyName);

                var fullAssemblyName = Path.GetFullPath(options.AssemblyName);
                if (options.Verbose) Console.WriteLine($"Document full assembly file name: {fullAssemblyName.Quoted()}");

                if (options.Verbose) AppDomain.CurrentDomain.AssemblyLoad += ShowAssemblyLoaded;
                AppDomain.CurrentDomain.AssemblyResolve +=
                    (sender, args) => ResolveAssembly(sender, args, options.Verbose, Path.GetDirectoryName(fullAssemblyName));

                var myAssembly = Assembly.LoadFile(fullAssemblyName);
                if (myAssembly == null)
                {
                    throw new Exception($"Could not load assembly {options.AssemblyName.Quoted()}");
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
                                $"Specified type name {options.TypeName.Quoted()} not found in assembly {options.AssemblyName.Quoted()}");

                        throw new Exception(
                            $"Specified type name {options.TypeName.Quoted()} not found in assembly {options.AssemblyName.Quoted()}." +
                            $" Similar type names in the assembly: {string.Join(",", possibleTypes)}");
                    }
                }
                var recursive = options.AllRecursive || options.RecursiveAssemblies.Any();
                if (options.AllRecursive) options.RecursiveAssemblies = new List<string>();

                var assembly = rootType == null ? myAssembly : null;
                var typeList = OrderedTypeList.LoadTypes(
                    rootType,
                    assembly,
                    recursive,
                    options.RecursiveAssemblies,
                    typeFilterOptions,
                    options.Verbose);
                #endregion

                var docOptions = new DocumentationGeneratorOptions()
                {
                    Strings = localizedStrings,
                    DocumentTitle = GenerateTitle(assembly, options.DocumentTitle, localizedStrings),
                    DocumentMethodDetails = options.DocumentMethodDetails,
                    ShowDocumentDateTime = !options.DoNotShowDocumentDateTime,
                    ShowCommandLine = options.ShowCommandLine,
                    AddMsdnLinks = !options.MsdnLinkViewParameter.IsNullOrEmpty(),
                    MsdnViewParameter = options.MsdnLinkViewParameter.IsNullOrEmpty() ||
                        options.MsdnLinkViewParameter.EqualsIgnoreCase("latest")
                        ? null : options.MsdnLinkViewParameter,
                    MsdnCultureName = localizedStrings.CultureName.ToLower()
                };

                var generator = new DocumentationGenerator(
                    typeList,
                    docOptions,
                    writer);

                generator.BuildDocument();

                // Write markdown to the output file
                File.WriteAllText(options.OutputFile, generator.FullText);
            }
            catch (BadImageFormatException exc)
            {
                WriteError($"Error: {exc.Message}");
                WriteError($"Hresult:{exc.HResult}");
                if (!exc.HelpLink.IsNullOrEmpty()) WriteError($"Help link: {exc.HelpLink}");
                WriteError($"{exc.StackTrace}");
            }
            catch (Exception exc)
            {
                WriteError($"Error: {exc.Message}");
                WriteError($"{exc.StackTrace}");
            }
        }

        /// <summary>
        /// Parse command line, validate options and prepare type filter options based on the include/exclude 
        /// filters specified in the command line.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A tuple containing the parsed command line options and the type filter options.</returns>
        static (CommandLineOptions.CommandLineOptions, TypeFilterOptions) Parse(string [] args)
        {
            var result = CliProgram.Parse(args);
            if (result.Tag == CommandLine.ParserResultType.NotParsed) return (null, null);

            try
            {
                CommandLineOptions.CommandLineOptions options = result.Value;
                if (string.IsNullOrEmpty(options.Format))
                {
                    options.Format = MarkdownWriters.First().Name;
                    Console.WriteLine($"Markdown format not specified. Assuming {options.Format}.");
                }
                if (!MarkdownWriters.Any(md => md.Name.EqualsIgnoreCase(options.Format)))
                {
                    WriteError($"Error: invalid markdown format specified. Valid values: {MarkdownFormatNames}");
                    return (null, null);
                }

                if (!string.IsNullOrEmpty(options.MsdnLinkViewParameter))
                {
                    List<string> validPrefixes = ["net-", "netcore-", "netframework-", "netstandard-", "dotnet-uwp-"];
                    if (!options.MsdnLinkViewParameter.EqualsIgnoreCase("latest") &&
                        !validPrefixes.Any(prefix => options.MsdnLinkViewParameter.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        WriteError($"Error: invalid MSDN view string. Expected \"latest\" or string starting with one of: {string.Join(" ", validPrefixes)}");
                        return (null, null);
                    }
                }

                if (!string.IsNullOrEmpty(options.OutputLanguage))
                {
                    if (!LocalizedStrings.Any(ls => ls.CultureName.EqualsIgnoreCase(options.OutputLanguage)))
                    {
                        WriteError($"Error: invalid language specified. Valid values: {LocalizedStringsCultureNames}");
                        return (null, null);
                    }
                }
                else if (!LocalizedStrings.Any(ls => ls.CultureName.EqualsIgnoreCase(CultureInfo.CurrentCulture.Name)))
                {
                    Console.WriteLine($"Assuming default language. No translations for current culture: {CultureInfo.CurrentCulture.Name}.");
                }

                if (options.IgnoreMethods) options.DocumentMethodDetails = false;
                if (options.AssemblyName != null && options.OutputFile == null)
                {
                    options.OutputFile = Path.GetFileNameWithoutExtension(options.AssemblyName) + ".md";
                }
                var typeFilter = TypeFilterOptions.Parse(options.IncludeFilters, options.ExcludeFilters);
                if (options.IgnoreMethods)
                {
                    typeFilter.Exclude.Add(new FilterItem() { FilterType = FilterType.Method, FilterScope = FilterScope.All });
                }
                foreach (var ignoreAttr in options.IgnoreAttributes)
                {
                    typeFilter.Exclude.Add(
                        new FilterItem()
                        {
                            FilterType = FilterType.All,
                            FilterScope = FilterScope.Attribute,
                            FilterParameter = ignoreAttr
                        });
                }
                return (options, typeFilter);
            }
            catch (Exception exc)
            {
                WriteError("Command line parse error: " + exc.Message);
                return (null, null);
            }
        }

        /// <summary>
        /// Show errors in red color in the console.
        /// </summary>
        /// <param name="error">The error message to display.</param>
        static public void WriteError(string error)
        {
            var oldBackgournd = Console.BackgroundColor;
            var oldForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(error);
            Console.ForegroundColor = oldForeground;
            Console.BackgroundColor = oldBackgournd;
            Console.WriteLine();
        }

        /// <summary>
        /// Generates a formatted title string for the documentation based on the assembly information.
        /// </summary>
        /// <param name="assembly">The assembly to extract name and version information from. Can be null.</param>
        /// <param name="format">The format string for the title. Supports {assembly} and {version} placeholders. Can be null.
        /// This format can be specified via the command line. If not specified, the function will use the default format of the document language.</param>
        /// <param name="localizedStrings">The localized strings provider for version prefix and default title format.</param>
        /// <returns>The formatted title string, or null if both format and assembly are null.</returns>
        public static string GenerateTitle(Assembly assembly, string format, ILocalizedStrings localizedStrings)
        {
            if (format == null && assembly == null) return null;
            var assemblyName = assembly == null ? "" : Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);
            var version = assembly == null ? "" : (localizedStrings.VersionPrefix + assembly.GetName().Version);
            if (format == null) format = localizedStrings.DefaultTitleFormat;
            return format.Replace("{assembly}", assemblyName).Replace("{version}", version);
        }

        #region Assembly resolution
        /// <summary>
        /// Loaded assemblies cache to prevent stack overflow from recursive assembly resolution attempts.
        /// </summary>
        private static Dictionary<string, Assembly> RequestedAssemblies { get; set; } = [];

        /// <summary>
        /// Resolves assembly references by loading them from the specified base path.
        /// Caches resolved assemblies to prevent stack overflow from recursive resolution attempts.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The event data containing the assembly name and requesting assembly.</param>
        /// <param name="verbose">If true, outputs detailed resolution information to the console.</param>
        /// <param name="basePath">The directory path where assemblies should be resolved from.</param>
        /// <returns>The resolved assembly, or null if resolution fails.</returns>
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
                    Console.WriteLine($"  Already loaded: {a.FullName}");
                }
                Console.WriteLine($"Loading: {fullAssemblyName}");
            }
            var assembly = Assembly.LoadFile(fullAssemblyName);
            RequestedAssemblies[shortAssemblyName] = assembly;
            return assembly;
        }

        private static void ShowAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine("Loaded assembly: " + args.LoadedAssembly.FullName);
            Console.WriteLine("File path: " + args.LoadedAssembly.GetName().FullName);
        }
        #endregion
    }
}
