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
using MdDox.MarkdownFormatters.Interfaces;
using MdDox.Reflection;
using System.Text;

namespace MdDox
{
    public class DocumentationGenerator
    {
        /// <summary>
        /// The ordered list of types to document.
        /// </summary>
        public OrderedTypeList TypeList;
        /// <summary>
        /// Document generator options.
        /// </summary>
        public DocumentationGeneratorOptions Options { get; }
        /// <summary>
        /// XML documentation reader. Finds compiler-generated documentation based on type reflection information.
        /// </summary>
        public DocXmlReader Reader { get; }
        /// <summary>
        /// Converts text to repository-specific markdown output.
        /// </summary>
        public IMarkdownFormatter Markdown { get; }
        /// <summary>
        /// Generated markdown text. Call BuildDocument() before retrieving the text.
        /// </summary>
        public string FullText => OutputText.ToString();

        Func<Type, Queue<string>, string> typeLinkConverter;
        StringBuilder OutputText { get; } = new StringBuilder();


        public DocumentationGenerator(
            OrderedTypeList typeList,
            DocumentationGeneratorOptions options,
            IMarkdownFormatter writer)
        {
            Reader = new DocXmlReader();
            TypeList = typeList;
            Options = options;
            Markdown = writer;

            typeLinkConverter = (type, _) => TypeNameWithLinks(type, options.MsdnLinks, options.MsdnView);
        }

        #region Top level document build functions
        public void BuildDocument()
        {
            OutputText.Clear();
            WriteDocumentTitle(Options.DocumentTitle);
            WritedDateLine();
            WriteTypeIndex(Options.TypeIndexTitle, Options.TypeIndexColumnCount);
            foreach (var typeData in TypeList.TypesToDocument)
            {
                WriteTypeDocumentation(typeData);
            }
        }

        public void WriteDocumentTitle(string titleText)
        {
            if (titleText == null) return;
            WriteBigTitle(titleText);
        }

        public void WritedDateLine()
        {
            if (!Options.ShowDocumentDateTime) return;
            WriteLine("Created by " + Markdown.Link("https://github.com/loxsmoke/mddox", "mddox") +
                $" on {DateTime.Now.ToShortDateString()}");
        }

        /// <summary>
        /// Write table of contents. It is a multi-column table with each cell containing 
        /// the link to the heading of the type.
        /// </summary>
        /// <param name="indexTitleText"></param>
        /// <param name="columnCount"></param>
        public void WriteTypeIndex(string indexTitleText, int columnCount)
        {
            if (TypeList.TypesToDocument.Count == 0) return;

            if (indexTitleText != null) WriteBigTitle(indexTitleText);
            var emptyRow = Enumerable.Repeat(" ", columnCount).ToArray();
            WriteTableTitle(emptyRow);
            for (var i = 0; i < TypeList.TypesToDocument.Count; i += columnCount)
            {
                var row = emptyRow.ToArray();
                for (var j = 0; j < columnCount && j + i < TypeList.TypesToDocument.Count; j++)
                {
                    var typeData = TypeList.TypesToDocument[i + j];
                    row[j] = Markdown.HeadingLink(TypeTitle(typeData.Type), TypeTitle(typeData.Type));
                }
                WriteTableRow(row);
            }
        }

        public void WriteTypeDocumentation(TypeCollection.TypeInformation typeData)
        {
            if (typeData.Type.IsEnum)
            {
                WriteEnumDocumentation(typeData);
            }
            else
            {
                WriteClassDocumentation(typeData);
            }
        }

        /// <summary>
        /// Write markdown documentation for the enum type:
        /// Examples, Remarks, 
        /// </summary>
        /// <param name="typeData"></param>
        public void WriteEnumDocumentation(TypeCollection.TypeInformation typeData)
        {
            WriteTypeTitle(typeData.Type);

            var enumComments = Reader.GetEnumComments(typeData.Type, true);
            WriteSummary(enumComments.Summary);
            WriteExample(enumComments.Example);
            WriteRemarks(enumComments.Remarks);

            if (enumComments.ValueComments.Count > 0)
            {
                WriteTitle("Values");
                WriteTableTitle("Name", "Summary");
                foreach (var prop in enumComments.ValueComments)
                {
                    WriteTableRow(Markdown.Bold(prop.Name), ProcessTags(prop.Summary));
                }
            }
        }

        /// <summary>
        /// Write markdown documentation for the class:
        /// Base class,  summary, remarks, Properties, constructors, methods and fields
        /// </summary>
        /// <param name="typeData"></param>
        public void WriteClassDocumentation(TypeCollection.TypeInformation typeData)
        {
            WriteTypeTitle(typeData.Type);

            if (typeData.Type.BaseType != null &&
                typeData.Type.BaseType != typeof(Object) &&
                typeData.Type.BaseType != typeof(ValueType))
            {
                WriteLine("Base class: " + typeData.Type.BaseType.ToNameString(typeLinkConverter, true));
            }

            var typeComments = Reader.GetTypeComments(typeData.Type);
            WriteSummary(typeComments.Summary);
            WriteExample(typeComments.Example);
            WriteRemarks(typeComments.Remarks);

            var allProperties = Reader.Comments(typeData.Properties).ToList();
            var allConstructors = Reader.Comments(typeData.Methods.Where(it => it is ConstructorInfo)).ToList();
            var allMethods = Reader.Comments(typeData.Methods
                .Where(it => !(it is ConstructorInfo) && (it is MethodInfo))).ToList();
            var allFields = Reader.Comments(typeData.Fields).ToList();
            if (allProperties.Count > 0)
            {
                WriteTitle("Properties");
                WriteTableTitle("Name", "Type", "Summary");
                foreach (var (Info, Comments) in allProperties)
                {
                    WriteTableTitle(
                        Markdown.Bold(Info.Name),
                        Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(Comments.Summary));
                }
            }

            if (allConstructors.Count > 0)
            {
                WriteTitle("Constructors");
                WriteTableTitle("Name", "Summary");
                foreach (var (Info, Comments) in allConstructors.OrderBy(m => m.Info.GetParameters().Length))
                {
                    var heading = typeData.Type.ToNameString() + Info.ToParametersString();
                    heading = Options.DocumentMethodDetails ? Markdown.HeadingLink(heading, Markdown.Bold(heading)) : Markdown.Bold(heading);
                    WriteTableRow(heading, ProcessTags(Comments.Summary));
                }
            }

            if (allMethods.Count > 0)
            {
                WriteTitle("Methods");
                WriteTableTitle("Name", "Returns", "Summary");
                foreach (var (Info, Comments) in allMethods
                    .OrderBy(m => m.Info.Name)
                    .ThenBy(m => m.Info.GetParameters().Length))
                {
                    var methodInfo = Info as MethodInfo;
                    var heading = methodInfo.Name + methodInfo.ToParametersString();
                    heading = Options.DocumentMethodDetails ? Markdown.HeadingLink(heading, Markdown.Bold(heading)) : Markdown.Bold(heading);
                    WriteTableRow(heading,
                        methodInfo.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(Comments.Summary));
                }
            }

            if (allFields.Count > 0)
            {
                WriteTitle("Fields");
                WriteTableTitle("Name", "Type", "Summary");
                foreach (var (Info, Comments) in allFields)
                {
                    WriteTableRow(
                        Markdown.Bold(Info.Name),
                        Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(Comments.Summary));
                }
            }

            if (Options.DocumentMethodDetails)
            {
                if (allConstructors.Count > 0)
                {
                    WriteTitle("Constructors");
                    foreach (var (info, comments) in allConstructors
                        .OrderBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(typeData.Type.ToNameString(), info, comments);
                    }
                }
                if (allMethods.Count > 0)
                {
                    WriteTitle("Methods");
                    foreach (var (info, comments) in allMethods
                        .OrderBy(m => m.Info.Name)
                        .ThenBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(info.Name, info, comments);
                    }
                }
            }
        }

        private void WriteMethodDetails(string name, MethodBase info, MethodComments comments)
        {
            WriteSmallTitle(name + info.ToParametersString());
            WriteSummary(comments.Summary);
            if (comments.Parameters.Count > 0)
            {
                var parameters = info.GetParameters();
                var i = 0;
                WriteTableTitle("Parameter", "Type", "Description");
                foreach (var (paramName, text) in comments.Parameters)
                {
                    WriteTableRow(paramName,
                        parameters[i++].ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(text));
                }
            }
            WriteLine("");

            if (info is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
            {
                WriteSmallTitle("Returns");
                WriteLine(methodInfo.ToTypeNameString(typeLinkConverter, true));
                WriteSummary(comments.Returns);
            }
            WriteExample(comments.Example);
        }
        #endregion

        #region Formatting, tags, links
        public string TypeNameWithLinks(Type type, bool msdnLinks, string msdnView)
        {
            if (TypeList.TypesToDocumentSet.Contains(type))
            {
                return Markdown.HeadingLink(TypeTitle(type), 
                    type.IsGenericTypeDefinition ?  type.Name.CleanGenericTypeName() : type.ToNameString());
            }
            if (msdnLinks &&
                type != typeof(string) &&
                (!type.IsValueType || type.IsEnum) &&
                (type.Assembly.ManifestModule.Name.StartsWith("System.") ||
                type.Assembly.ManifestModule.Name.StartsWith("Microsoft.")))
            {
                return Markdown.Link(MsdnUrlForType(type, msdnView),
                    type.IsGenericTypeDefinition ? type.Name.CleanGenericTypeName() : type.ToNameString());
            }
            if (type.IsGenericTypeDefinition)
            {
                return $"{type.Name.CleanGenericTypeName()}";
            }
            return null;
        }

        static (string cref, string innerText, string beforeText, string afterText) FindTagWithAttribute(
            string text, string tag, string attributeName)
        {
            if (text.IsNullOrEmpty() || !text.Contains(tag)) return (null, null, text, null);
            var simpleTag = new Regex("<" + tag + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
            var match = simpleTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, "", text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }

            var bigTag = new Regex("<" + tag + "( +)"+ attributeName + "( *)=( *)\"(.*?)\"( *)>(.*?)</" + tag + ">");
            match = bigTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, match.Groups[6].Value, text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }
            return (null, null, text, null);
        }

        string ProcessTags(string text)
        {
            for (; ; )
            {
                var (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "seealso", "cref");
                if (cref != null)
                {
                    text = beforeText + Markdown.Bold(FixCref(cref)) + afterText;
                    continue;
                }
                (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "see", "cref");
                if (cref != null)
                {
                    text = beforeText + Markdown.Bold(FixCref(cref)) + afterText;
                    continue;
                }
                (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "see", "href");
                if (cref != null)
                {
                    text = beforeText + $" {Markdown.Link(cref, innerText )} " + afterText;
                    continue;
                }

                if (text == null) return text;
                text = text
                    .RegexReplace(@"\s*</para>\s*<para>\s*", Markdown.NewLine)
                    .RegexReplace(@"\s*<para>\s*", Markdown.NewLine)
                    .RegexReplace(@"\s*</para>\s*", Markdown.NewLine)
                    .Trim();

                if (text == null) return text;
                text = text
                    .Replace("<c>", Markdown.StartInlineCode)
                    .Replace("</c>", Markdown.EndInlineCode)
                    .Replace("<code>", Markdown.StartMultilineCode)
                    .Replace("</code>", Markdown.EndMultilineCode)
                    .Trim();
                return text;
            }
        }

        static string FixCref(string crefText)
        {
            if (!crefText.Contains(":")) return crefText;
            // XML doc Id
            return crefText.Substring(crefText.IndexOf(":") + 1);
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
            var urlParameters = view.IsNullOrEmpty() ? "" : $"?view={view}";
            var typeNameFragment = type.FullName.ToLowerInvariant();
            if (typeNameFragment.Contains('`')) typeNameFragment = typeNameFragment.Replace('`', '-');
            var url = $"https://docs.microsoft.com/{docLocale}/dotnet/api/{typeNameFragment}{urlParameters}";
            return url;
        }

        static string TypeTitle(Type type)
        {
            return type.ToNameString() + (type.IsEnum ? " Enum" : (type.IsValueType ? " Struct" : " Class"));
        }
        #endregion

        #region Simple formatted write functions
        public void WriteTypeTitle(Type type)
        {
            WriteBigTitle(TypeTitle(type));
            WriteLine("Namespace: " + type.Namespace);
        }

        public void WriteSummary(string summary)
        {
            WriteLine(ProcessTags(summary));
        }

        public void WriteExample(string example)
        {
            if (example.IsNullOrEmpty()) return;

            WriteTitle("Examples");
            WriteLine(ProcessTags(example));
        }
        public void WriteRemarks(string remarks)
        {
            if (remarks.IsNullOrEmpty()) return;

            WriteTitle("Remarks");
            WriteLine(ProcessTags(remarks));
        }
        #endregion

        #region Low level write functions
        public void WriteBigTitle(string title)
        {
            OutputText.Append(Markdown.Heading(1, title));
        }
        public void WriteTitle(string title)
        {
            OutputText.Append(Markdown.Heading(2, title));
        }
        public void WriteSmallTitle(string title)
        {
            OutputText.Append(Markdown.Heading(3, title));
        }
        public void WriteTableTitle(params string[] tableHeadings)
        {
            OutputText.Append(Markdown.TableTitle(tableHeadings));
        }
        public void WriteTableRow(params string[] row)
        {
            OutputText.Append(Markdown.TableRow(row));
        }
        public void WriteLine(string text)
        {
            OutputText.Append(Markdown.WithNewline(text));
        }
        #endregion
    }
}
