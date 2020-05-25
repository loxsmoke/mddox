using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DocXml.Reflection;
using LoxSmoke.DocXml;
using LoxSmoke.DocXml.Reflection;
using static LoxSmoke.DocXml.Reflection.DocXmlReaderExtensions;
using static DocXml.Reflection.ReflectionExtensions;
using MdDox.MarkdownWriters.Interfaces;

namespace MdDox
{
    public class DocumentationGenerator
    {
        public DocXmlReader Reader { get; }
        public IMarkdownWriter Writer { get; }
        public TypeCollection TypeCollection { get; }
        public List<TypeCollection.TypeInformation> TypesToDocument { get; }
        public HashSet<Type> TypesToDocumentSet { get; set; }
        private Func<Type, Queue<string>, string> typeLinkConverter;

        public DocumentationGenerator(
            IMarkdownWriter writer,
            TypeCollection typeCollection,
            Type firstType = null,
            bool msdnLinks = false,
            string msdnView = null)
        {
            Reader = new DocXmlReader();
            Writer = writer;
            TypeCollection = typeCollection;
            TypesToDocument = typeCollection.ReferencedTypes.Values
                .OrderBy(t => t.Type.Namespace)
                .ThenBy(t => t.Type.Name).ToList();
            if (firstType != null)
            {
                var typeDesc = TypesToDocument.FirstOrDefault(t => t.Type == firstType);
                if (typeDesc != null)
                {
                    TypesToDocument.Remove(typeDesc);
                    TypesToDocument.Insert(0, typeDesc);
                }
            }

            TypesToDocumentSet = new HashSet<Type>(TypesToDocument.Select(t => t.Type));
            typeLinkConverter = (type, _) =>
            {
                if (TypesToDocumentSet.Contains(type))
                {
                    return Writer.HeadingLink(TypeTitle(type), type.ToNameString());
                }
                if (msdnLinks &&
                    type != typeof(string) &&
                    !type.IsValueType &&
                    (type.Assembly.ManifestModule.Name.StartsWith("System.") ||
                    type.Assembly.ManifestModule.Name.StartsWith("Microsoft.")))
                {
                    return Writer.Link(MsdnUrlForType(type, msdnView),
                        type.IsGenericTypeDefinition ? type.Name.CleanGenericTypeName() : type.ToNameString());
                }
                if (type.IsGenericTypeDefinition)
                {
                    return $"{type.Name.CleanGenericTypeName()}";
                }
                return null;
            };
        }

        public static void GenerateMarkdown(
            Assembly assembly,
            IMarkdownWriter writer,
            string outputFileName,
            List<string> ignoreAttributes,
            bool ignoreMethods,
            bool recursive,
            bool msdnLinks,
            string msdnView,
            bool showDateLine,
            bool verbose)
        {
            GenerateMarkdown(null, assembly, recursive, null, ignoreAttributes, ignoreMethods, msdnLinks, msdnView, showDateLine, verbose,
                writer, outputFileName);
        }

        public static void GenerateMarkdown(
            Type rootType,
            IMarkdownWriter writer,
            string outputFileName,
            List<string> ignoreAttributes,
            bool ignoreMethods,
            bool recursive,
            bool msdnLinks,
            string msdnView,
            bool showDateLine,
            bool verbose)
        {
            GenerateMarkdown(rootType, null, recursive, null, ignoreAttributes, ignoreMethods, msdnLinks, msdnView, showDateLine, verbose,
                writer, outputFileName);
        }

        static bool HasIgnoreAttribute(PropertyInfo info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            var customAttributes = info.GetCustomAttributes().ToList();
            return info.GetCustomAttributes().Any(attr => ignoreAttributes.Contains(attr.GetType().Name));
        }
        static bool HasIgnoreAttribute(MethodBase info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            var customAttributes = info.GetCustomAttributes().ToList();
            return info.GetCustomAttributes().Any(attr => ignoreAttributes.Contains(attr.GetType().Name));
        }
        static bool HasIgnoreAttribute(Type info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            var customAttributes = info.GetCustomAttributes().ToList();
            return info.GetCustomAttributes().Any(attr => ignoreAttributes.Contains(attr.GetType().Name));
        }

        public static void GenerateMarkdown(
            Type rootType,
            Assembly assembly,
            bool recursiveAssemblyTraversal,
            List<string> recursiveAssemblies,
            List<string> ignoreAttributes,
            bool ignoreMethods,
            bool msdnLinks,
            string msdnView,
            bool showDateLine,
            bool verbose,
            IMarkdownWriter markdownWriter,
            string outputFileName)
        {
            // Reflection setup
            var allAssemblyTypes = assembly != null;
            if (assembly == null) assembly = rootType.Assembly;
            var ignoreAttributesSet = ignoreAttributes == null || ignoreAttributes.Count == 0 ? null : new HashSet<string>(ignoreAttributes);

            if (recursiveAssemblies != null && recursiveAssemblies.Count == 0) recursiveAssemblies = null;

            if (verbose)
            {
                if (assembly != null) Log(assembly, "Root assembly ");
            }

            var reflectionSettings = ReflectionSettings.Default;
            reflectionSettings.PropertyFilter = info => PropertyFilter(info, ignoreAttributesSet, verbose);
            reflectionSettings.MethodFilter = info => MethodFilter(info, ignoreMethods, ignoreAttributesSet, verbose);
            reflectionSettings.TypeFilter = type => TypeFilter(type, ignoreAttributesSet, verbose);
            reflectionSettings.AssemblyFilter =
                reflectionAssembly => AssemblyFilter(reflectionAssembly, assembly, recursiveAssemblies, recursiveAssemblyTraversal, verbose);

            // Reflection
            var typeCollection = allAssemblyTypes ?
                TypeCollection.ForReferencedTypes(assembly, reflectionSettings) :
                TypeCollection.ForReferencedTypes(rootType, reflectionSettings);

            // Generate markdown
            var generator = new DocumentationGenerator(markdownWriter, typeCollection, rootType, msdnLinks, msdnView);
            if (assembly != null) generator.WriteDocumentTitle(assembly);
            if (showDateLine) generator.WritedDateLine();
            generator.WriteTypeIndex();
            generator.DocumentTypes();

            // Write markdown to the output file
            File.WriteAllText(outputFileName, generator.Writer.FullText);
        }

        #region Filters and logging
        public static bool PropertyFilter(PropertyInfo info, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            var document = !HasIgnoreAttribute(info, ignoreAttributesSet);
            if (verbose)
            {
                Log(info, (document ? "Document " : "Ignore by attribute ") + "property ");
            }
            return document;
        }

        public static void Log(PropertyInfo info, string message)
        {
            Console.WriteLine("    " + message + info.ToTypeNameString() + " " + info.Name);
        }

        public static bool MethodFilter(MethodBase info, bool ignoreMethods, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            if (ignoreMethods) return false;
            var document = !HasIgnoreAttribute(info, ignoreAttributesSet);
            if (verbose)
            {
                Log(info, (document ? "Document " : "Ignore by attribute ") + "method ");
            }
            return document;
        }

        public static void Log(MethodBase info, string message)
        {
            Console.WriteLine("    " + message + info.Name + info.ToParametersString());
        }

        public static bool TypeFilter(Type type, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            if (HasIgnoreAttribute(type, ignoreAttributesSet))
            {
                if (verbose) Log(type, "Ignore by attribute ");
                return false;
            }
            if (verbose) Log(type, "Document type ");
            return true;
        }

        public static void Log(Type type, string message)
        {
            Console.WriteLine("  " + message + type.Namespace + "." +  type.ToNameString());
        }

        public static bool AssemblyFilter(
            Assembly assembly,
            Assembly rootAssembly,
            List<string> recursiveAssemblies, 
            bool recursiveAssemblyTraversal, 
            bool verbose)
        {
            if (assembly == rootAssembly) return true;

            if (!recursiveAssemblyTraversal)
            {
                if (!verbose) return false;
                Log(assembly, "No recursive traversal. Ignoring ");
                return false;
            }

            if (recursiveAssemblies == null) return true;

            if (!recursiveAssemblies.Any(name => name.Equals(Path.GetFileName(assembly.Location), StringComparison.OrdinalIgnoreCase)))
            {
                if (!verbose) return false;
                Log(assembly, "Assembly not in the list. Ignoring ");
                return false;
            }
            if (File.Exists(Path.ChangeExtension(assembly.Location, ".xml"))) return true;
            if (!verbose) return false;
            Log(assembly, "No xml file for the assembly. Ignoring ");
            return false;
        }

        public static void Log(Assembly assembly, string message)
        {
            Console.WriteLine(message + assembly.FullName);
            Console.WriteLine("File path: " + assembly.Location);
        }
        #endregion

        public void WriteDocumentTitle(Assembly assembly, string titleText = "API documentation")
        {
            Writer.WriteH1($"{Path.GetFileName(assembly.ManifestModule.Name)} v.{assembly.GetName().Version} " +
                           titleText ?? "");
        }

        public void WritedDateLine()
        {
            Writer.Write("Created by ");
            Writer.WriteLink("https://github.com/loxsmoke/mddox", "mddox");
            Writer.WriteLine($" on {DateTime.Now.ToShortDateString()}");
        }

        static string TypeTitle(Type type)
        {
            return type.ToNameString() + (type.IsEnum ? " Enum" : (type.IsValueType ? " Struct" : " Class"));
        }

        static (string cref, string innerText, string beforeText, string afterText) FindTagWithCref(string text, string tag)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(tag)) return (null, null, text, null);
            var simpleTag = new Regex("<" + tag + "( +)cref( *)=( *)\"(.*?)\"( *)/>");
            var match = simpleTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, "", text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }

            var bigTag = new Regex("<" + tag + "( +)cref( *)=( *)\"(.*?)\"( *)>(.*?)</" + tag + ">");
            match = bigTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, "", text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }
            return (null, null, text, null);
        }

        static string ProcessTags(string text)
        {
            for (; ; )
            {
                var (cref, innerText, beforeText, afterText) = FindTagWithCref(text, "seealso");
                if (cref != null)
                {
                    text = beforeText + "**" + FixCref(cref) + "**" + afterText;
                    continue;
                }
                (cref, innerText, beforeText, afterText) = FindTagWithCref(text, "see");
                if (cref != null)
                {
                    text = beforeText + "**" + FixCref(cref) + "**" + afterText;
                    continue;
                }

                return RemoveParaTags(text);
            }
        }

        static string FixCref(string crefText)
        {
            if (crefText.Contains(":")) // XML doc Id
            {
                return crefText.Substring(crefText.IndexOf(":") + 1);
            }
            return crefText;
        }

        /// <summary>
        /// Generate URL to the documentation page of the type at https://docs.microsoft.com/
        /// </summary>
        /// <param name="type">The type to generate url for</param>
        /// <param name="view">The documentation framework version parameter.
        /// For example netcore-3.1, netframework-4.8, netstandard-2.1, and so on.
        /// If not specified then view parameter is omitted.</param>
        /// <returns>URL to the type documentation page</returns>
        static string MsdnUrlForType(Type type, string view = null)
        {
            var docLocale = "en-us";
            var urlParameters = string.IsNullOrEmpty(view) ? "" : $"?view={view}";
            var typeNameFragment = type.FullName.ToLowerInvariant();
            if (typeNameFragment.Contains('`')) typeNameFragment = typeNameFragment.Replace('`', '-');
            var url = $"https://docs.microsoft.com/{docLocale}/dotnet/api/{typeNameFragment}{urlParameters}";
            return url;
        }

        static string RemoveParaTags(string text) => text?
            .RegexReplace(@"\s*</para>\s*<para>\s*", "\r\n\r\n")
            .RegexReplace(@"\s*<para>\s*", "\r\n\r\n")
            .RegexReplace(@"\s*</para>\s*", "\r\n\r\n")
            .Trim();

        /// <summary>
        /// Write table of contents. It is a three column table with each cell containing 
        /// the link to the heading of the type.
        /// </summary>
        /// <param name="indexTitleText"></param>
        public void WriteTypeIndex(string indexTitleText = "All types")
        {
            var namesForTOC = TypesToDocument
                .Select(typeData => Writer.HeadingLink(TypeTitle(typeData.Type), TypeTitle(typeData.Type))).ToList();
            if (namesForTOC.Count == 0) return;

            if (indexTitleText != null) Writer.WriteH1(indexTitleText);
            Writer.WriteTableTitle(" ", " ", " ");
            var rowCount = namesForTOC.Count / 3 + (((namesForTOC.Count % 3) == 0) ? 0 : 1);
            for (var i = 0; i < rowCount; i++)
            {
                Writer.WriteTableRow(namesForTOC[i],
                    rowCount + i < namesForTOC.Count ? namesForTOC[rowCount + i] : " ",
                    rowCount * 2 + i < namesForTOC.Count ? namesForTOC[rowCount * 2 + i] : " ");
            }
        }

        /// <summary>
        /// Write markdown documentation for the enum type:
        /// Examples, Remarks, 
        /// </summary>
        /// <param name="enumType"></param>
        public void DocumentEnum(Type enumType)
        {
            Writer.WriteH1(TypeTitle(enumType));
            Writer.WriteLine("Namespace: " + enumType.Namespace);

            var enumComments = Reader.GetEnumComments(enumType, true);
            Writer.WriteLine(ProcessTags(enumComments.Summary));

            if (!string.IsNullOrEmpty(enumComments.Example))
            {
                Writer.WriteH2("Examples");
                Writer.WriteLine(ProcessTags(enumComments.Example));
            }

            if (!string.IsNullOrEmpty(enumComments.Remarks))
            {
                Writer.WriteH2("Remarks");
                Writer.WriteLine(ProcessTags(enumComments.Remarks));
            }

            if (enumComments.ValueComments.Count > 0)
            {
                Writer.WriteH2("Values");
                Writer.WriteTableTitle("Name", "Summary");
                foreach (var prop in enumComments.ValueComments)
                {
                    Writer.WriteTableRow(Writer.Bold(prop.Name),
                        ProcessTags(prop.Summary));
                }
            }
        }

        /// <summary>
        /// Write markdown documentation for the class:
        /// Base class,  summary, remarks, Properties, constructors, methods and fields
        /// </summary>
        /// <param name="typeData"></param>
        public void DocumentClass(TypeCollection.TypeInformation typeData)
        {
            Writer.WriteH1(TypeTitle(typeData.Type));
            Writer.WriteLine("Namespace: " + typeData.Type.Namespace);

            if (typeData.Type.BaseType != null &&
                typeData.Type.BaseType != typeof(Object) &&
                typeData.Type.BaseType != typeof(ValueType))
            {
                Writer.WriteLine("Base class: " + typeData.Type.BaseType.ToNameString(typeLinkConverter, true));
            }

            var typeComments = Reader.GetTypeComments(typeData.Type);
            Writer.WriteLine(typeComments.Summary);

            if (!string.IsNullOrEmpty(typeComments.Example))
            {
                Writer.WriteH2("Examples");
                Writer.WriteLine(ProcessTags(typeComments.Example));
            }

            if (!string.IsNullOrEmpty(typeComments.Remarks))
            {
                Writer.WriteH2("Remarks");
                Writer.WriteLine(ProcessTags(typeComments.Remarks));
            }

            var allProperties = Reader.Comments(typeData.Properties).ToList();
            var allMethods = Reader.Comments(typeData.Methods).ToList();
            var allFields = Reader.Comments(typeData.Fields).ToList();

            if (allProperties.Count > 0)
            {
                Writer.WriteH2("Properties");
                Writer.WriteTableTitle("Name", "Type", "Summary");
                foreach (var prop in allProperties)
                {
                    Writer.WriteTableRow(
                        Writer.Bold(prop.Info.Name),
                        prop.Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(prop.Comments.Summary));
                }
            }

            if (allMethods.Count > 0 && allMethods.Any(m => m.Info is ConstructorInfo))
            {
                Writer.WriteH2("Constructors");
                Writer.WriteTableTitle("Name", "Summary");
                foreach (var ctor in allMethods
                    .Where(m => m.Info is ConstructorInfo)
                    .OrderBy(m => m.Info.GetParameters().Length))
                {
                    Writer.WriteTableRow(
                        Writer.Bold(typeData.Type.ToNameString() + ctor.Info.ToParametersString(typeLinkConverter, true)),
                        ProcessTags(ctor.Comments.Summary));
                }
            }

            if (allMethods.Count > 0 && allMethods.Any(m => m.Info is MethodInfo))
            {
                Writer.WriteH2("Methods");
                Writer.WriteTableTitle("Name", "Returns", "Summary");
                foreach (var method in allMethods
                    .Where(m => m.Info != null && !(m.Info is ConstructorInfo) && (m.Info is MethodInfo))
                    .OrderBy(m => m.Info.Name)
                    .ThenBy(m => m.Info.GetParameters().Length))
                {
                    var methodInfo = method.Info as MethodInfo;
                    Writer.WriteTableRow(
                        Writer.Bold(methodInfo.Name + methodInfo.ToParametersString(typeLinkConverter, true)),
                        methodInfo.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(method.Comments.Summary));
                }
            }

            if (allFields.Count > 0)
            {
                Writer.WriteH2("Fields");
                Writer.WriteTableTitle("Name", "Type", "Summary");
                foreach (var field in allFields)
                {
                    Writer.WriteTableRow(
                        Writer.Bold(field.Info.Name),
                        field.Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(field.Comments.Summary));
                }
            }
        }

        public void DocumentTypes()
        {
            foreach (var typeData in TypesToDocument)
            {
                if (typeData.Type.IsEnum)
                {
                    DocumentEnum(typeData.Type);
                    continue;
                }
                DocumentClass(typeData);
            }
        }
    }
}
