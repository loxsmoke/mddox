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
using MdDox.Localization.Interfaces;

namespace MdDox
{
    public class DocumentationGenerator
    {
        #region Properties
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

        Func<Type, Queue<string>, string> TypeLinkConverter { get; }
        StringBuilder OutputText { get; } = new StringBuilder();
        #endregion

        public DocumentationGenerator(
            OrderedTypeList typeList,
            DocumentationGeneratorOptions options,
            IMarkdownFormatter writer)
        {
            Reader = new DocXmlReader();
            TypeList = typeList;
            Options = options;
            Markdown = writer;

            TypeLinkConverter = (type, _) => TypeNameWithLinks(type, options.AddMsdnLinks, options.MsdnViewParameter);
        }

        #region Top level document build functions
        public void BuildDocument()
        {
            OutputText.Clear();
            WriteDocumentTitle(Options.DocumentTitle);
            WritedDateLine();
            WriteTypeIndex(Options.Strings.AllTypes, Options.TypeIndexColumnCount);
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
            if (Options.ShowDocumentDateTime)
            {
                WriteLine(Options.Strings.CreatedBy + Markdown.Link("https://github.com/loxsmoke/mddox", "mddox") +
                    $"{Options.Strings.CreatedByOn}{DateTime.Now.ToShortDateString()}");
            }
            if (Options.ShowCommandLine)
            {
                // clear command line. Remove the first file name argument
                var simplifiedCommandLine = Environment.GetCommandLineArgs()
                    .Select(f => f.StartsWith('-') || !(f.Contains('\\') || f.Contains('/')) ? f : Path.GetFileName(f))
                    .ToList();
                WriteLine(Options.Strings.CommandLine + string.Join(" ", simplifiedCommandLine));
            }
        }

        /// <summary>
        /// Write table of contents. It is a multi-column table with each cell containing 
        /// the link to the heading of the type. For example: "SomeType Class"
        /// </summary>
        /// <param name="indexTitleText">The title text above the table</param>
        /// <param name="columnCount">The number of columns in the table</param>
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
                    row[j] = Markdown.HeadingLink(TypeTitle(typeData.Type, Options.Strings), TypeTitle(typeData.Type, Options.Strings));
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
        /// Summary, Examples, Remarks
        /// </summary>
        /// <param name="typeData"></param>
        public void WriteEnumDocumentation(TypeCollection.TypeInformation typeData)
        {
            // Example: "MyClass Struct", "Namespace: MyNamespace"
            WriteTypeTitle(typeData.Type);

            var enumComments = Reader.GetEnumComments(typeData.Type, true);
            WriteSummary(enumComments.Summary);
            WriteExample(enumComments.Example);
            WriteRemarks(enumComments.Remarks);

            if (enumComments.ValueComments.Count > 0)
            {
                WriteTitle(Options.Strings.Values);
                WriteTableTitle(Options.Strings.Name, Options.Strings.Summary);
                foreach (var prop in enumComments.ValueComments)
                {
                    WriteTableRow(Markdown.Bold(prop.Name), Markdown.EscapeSpecialChars(ProcessTags(prop.Summary)));
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

            // Write base class if it is useful
            if (typeData.Type.BaseType != null &&
                typeData.Type.BaseType != typeof(Object) &&
                typeData.Type.BaseType != typeof(ValueType))
            {
                WriteLine(Options.Strings.BaseClass + typeData.Type.BaseType.ToNameString(TypeLinkConverter, true));
            }

            var typeComments = Reader.GetTypeComments(typeData.Type);
            WriteSummary(typeComments.Summary);
            WriteExample(typeComments.Example);
            WriteRemarks(typeComments.Remarks);

            var allProperties = Reader.Comments(typeData.Properties).ToList();
            var allConstructors = Reader.Comments(typeData.Methods.Where(it => it is ConstructorInfo)).ToList();
            var allMethods = Reader.Comments(typeData.Methods
                .Where(it => it is not ConstructorInfo && (it is MethodInfo))).ToList();
            var allFields = Reader.Comments(typeData.Fields).ToList();

            if (allProperties.Count > 0)
            {
                WriteTitle(Options.Strings.Properties);
                WriteTableTitle(Options.Strings.Name, Options.Strings.Type, Options.Strings.Summary);
                foreach (var (Info, Comments) in allProperties)
                {
                    WriteTableRow(
                        Markdown.Bold(Info.Name),
                        Info.ToTypeNameString(TypeLinkConverter, true),
                        Markdown.EscapeSpecialChars(ProcessTags(Comments.Summary)));
                }
            }

            if (allConstructors.Count > 0)
            {
                WriteTitle(Options.Strings.Constructors);
                WriteTableTitle(Options.Strings.Name, Options.Strings.Summary);
                foreach (var (Info, Comments) in allConstructors.OrderBy(m => m.Info.GetParameters().Length))
                {
                    var heading = typeData.Type.ToNameString() + Info.ToParametersString(TypeLinkConverter, true);
                    heading = Options.DocumentMethodDetails ? 
                        Markdown.HeadingLink(GetMethodTitleNoFormatting(Info, typeData.Type.ToNameString())) : 
                        Markdown.Bold(heading);
                    WriteTableRow(heading, Markdown.EscapeSpecialChars(ProcessTags(Comments.Summary)));
                }
            }

            if (allMethods.Count > 0)
            {
                WriteTitle(Options.Strings.Methods);
                WriteTableTitle(Options.Strings.Name, Options.Strings.Returns, Options.Strings.Summary);
                foreach (var (info, comments) in allMethods
                    .OrderBy(m => m.Info.Name)
                    .ThenBy(m => m.Info.GetParameters().Length))
                {
                    var methodInfo = info as MethodInfo;
                    var heading = methodInfo.Name + methodInfo.ToParametersString(TypeLinkConverter, true);
                    heading = Options.DocumentMethodDetails ? 
                        Markdown.HeadingLink(GetMethodTitleNoFormatting(methodInfo)) : 
                        Markdown.Bold(heading);
                    WriteTableRow(heading,
                        methodInfo.ToTypeNameString(TypeLinkConverter, true),
                        Markdown.EscapeSpecialChars(ProcessTags(comments.Summary)));
                }
            }

            if (allFields.Count > 0)
            {
                WriteTitle(Options.Strings.Fields);
                WriteTableTitle(Options.Strings.Name, Options.Strings.Type, Options.Strings.Summary);
                foreach (var (Info, Comments) in allFields)
                {
                    WriteTableRow(
                        Markdown.Bold(Info.Name),
                        Info.ToTypeNameString(TypeLinkConverter, true),
                        Markdown.EscapeSpecialChars(ProcessTags(Comments.Summary)));
                }
            }

            if (Options.DocumentMethodDetails)
            {
                if (allConstructors.Count > 0)
                {
                    WriteTitle(Options.Strings.Constructors);
                    foreach (var (info, comments) in allConstructors
                        .OrderBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(typeData.Type.ToNameString(), info, comments);
                    }
                }
                if (allMethods.Count > 0)
                {
                    WriteTitle(Options.Strings.Methods);
                    foreach (var (info, comments) in allMethods
                        .OrderBy(m => m.Info.Name)
                        .ThenBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(info.Name, info, comments);
                    }
                }
            }
        }

        private string GetMethodTitleNoFormatting(MethodBase info, string methodName = null)
        {
            return (methodName ?? info.Name) + info.ToParametersString();
        }

        private void WriteMethodDetails(string name, MethodBase info, MethodComments comments)
        {
            // This title also serves as an anchor for the method
            WriteSmallTitle(GetMethodTitleNoFormatting(info, name));
            WriteSummary(comments.Summary);
            if (comments.Parameters.Count > 0)
            {
                var parameters = info.GetParameters();
                var i = 0;
                WriteTableTitle(Options.Strings.Parameter, Options.Strings.Type, Options.Strings.Description);
                foreach (var (paramName, text) in comments.Parameters)
                {
                    WriteTableRow(paramName,
                        parameters[i++].ToTypeNameString(TypeLinkConverter, true),
                        Markdown.EscapeSpecialChars(ProcessTags(text)));
                }
            }
            WriteLine("");

            if (info is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
            {
                WriteSmallTitle(Options.Strings.Returns);
                WriteLine(methodInfo.ToTypeNameString(TypeLinkConverter, true));
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
                return Markdown.HeadingLink(TypeTitle(type, Options.Strings), 
                    type.IsGenericTypeDefinition ?  type.Name.CleanGenericTypeName() : type.ToNameString());
            }
            if (msdnLinks &&
                type.FullName != null && // Open generic types may have null FullName.Just ignore them
                type != typeof(string) &&
                (!type.IsValueType || type.IsEnum) &&
                (type.Assembly.ManifestModule.Name.StartsWith("System.") ||
                type.Assembly.ManifestModule.Name.StartsWith("Microsoft.")))
            {
                return Markdown.Link(MsdnUrlForType(type, msdnView, Options.MsdnCultureName),
                    type.IsGenericTypeDefinition ? type.Name.CleanGenericTypeName() : type.ToNameString());
            }
            if (type.IsGenericTypeDefinition)
            {
                return $"{type.Name.CleanGenericTypeName()}";
            }
            return null;
        }

        /// <summary>
        /// Replace tags with attribute values or inner text with markdown formatting.
        /// Tags that are processed:
        /// TAG: seealso    VALUE: cref
        /// TAG: see        VALUE: cref
        /// TAG: see        VALUE: href
        /// TAG: paramref   VALUE: name
        /// TAG: code       INNER TEXT
        /// TAG: c          INNER TEXT
        /// TAG: para       newline
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string ProcessTags(string text)
        {
            for (; ; )
            {
                var (attributeValue, innerText, beforeText, afterText) = text.FindTagWithAttribute("seealso", "cref");
                if (attributeValue != null)
                {
                    text = beforeText + Markdown.Bold(FixCref(attributeValue)) + afterText;
                    continue;
                }
                (attributeValue, _, beforeText, afterText) = text.FindTagWithAttribute("see", "cref");
                if (attributeValue != null)
                {
                    text = beforeText + Markdown.Bold(FixCref(attributeValue)) + afterText;
                    continue;
                }
                (attributeValue, innerText, beforeText, afterText) = text.FindTagWithAttribute("see", "href");
                if (attributeValue != null)
                {
                    text = beforeText + $" {Markdown.Link(attributeValue, innerText )} " + afterText;
                    continue;
                }
                (attributeValue, _, beforeText, afterText) = text.FindShortTagWithAttribute("paramref", "name");
                if (attributeValue != null)
                {
                    text = beforeText + Markdown.Bold(FixCref(attributeValue)) + afterText;
                    continue;
                }
                // Multiline code
                (_, innerText, beforeText, afterText) = text.FindLongTagWithAttribute("code");
                if (attributeValue != null)
                {
                    text = beforeText + Markdown.StartMultilineCode +
                        Markdown.EscapeSpecialChars(innerText) +
                        Markdown.EndMultilineCode +
                        afterText;
                    continue;
                }

                // Inline code
                (_, innerText, beforeText, afterText) = text.FindLongTagWithAttribute("c");
                if (attributeValue != null)
                {
                    text = beforeText + Markdown.StartInlineCode +
                        Markdown.EscapeSpecialChars(innerText) +
                        Markdown.EndInlineCode +
                        afterText;
                    continue;
                }

                if (text == null) return text;

                text = text
                    .RegexReplace(@"\s*</para>\s*<para>\s*", Markdown.NewLine)
                    .RegexReplace(@"\s*<para>\s*", Markdown.NewLine)
                    .RegexReplace(@"\s*</para>\s*", Markdown.NewLine)
                    .Trim();
                return text;
            }
        }

        /// <summary>
        /// If cref contains a colon then it is a XML doc Id. Extract the Id part after ":".
        /// </summary>
        /// <param name="crefText"></param>
        /// <returns>Part after colon or entire text if colon is missing</returns>
        static string FixCref(string crefText)
        {
            if (!crefText.Contains(':')) return crefText;
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
        /// <param name="locale">The locale of the documentation.
        /// For example en-us</param>
        /// <returns>URL to the type documentation page</returns>
        static string MsdnUrlForType(Type type, string view, string locale)
        {
            var urlParameters = view.IsNullOrEmpty() ? "" : $"?view={view}";
            var typeNameFragment = type.FullName.ToLowerInvariant();
            if (typeNameFragment.Contains('`')) typeNameFragment = typeNameFragment.Replace('`', '-');
            var url = $"https://docs.microsoft.com/{locale.ToLower()}/dotnet/api/{typeNameFragment}{urlParameters}";
            return url;
        }

        /// <summary>
        /// Return the name of the type with localized version of Class, Struct, Interface, or Enum.
        /// For example: typeof(string) returns "string Class"
        /// </summary>
        /// <param name="type">Type to document</param>
        /// <param name="localizedStrings"></param>
        /// <returns>TypeName Type</returns>
        static string TypeTitle(Type type, ILocalizedStrings localizedStrings)
        {
            string complement;
            if (type.IsEnum) complement = localizedStrings.Enum;
            else if (type.IsInterface) complement = localizedStrings.Interface;
            else if (type.IsValueType) complement = localizedStrings.Struct;
            else complement = localizedStrings.Class;

            return type.ToNameString() + complement;
        }
        #endregion

        #region Simple formatted write functions
        /// <summary>
        /// Write the title and the namespace:
        /// Example: 
        /// #string Class
        /// Namespace: System
        /// </summary>
        /// <param name="type"></param>
        public void WriteTypeTitle(Type type)
        {
            WriteBigTitle(TypeTitle(type, Options.Strings));
            WriteLine(Options.Strings.Namespace + type.Namespace);
        }

        public void WriteSummary(string summary)
        {
            WriteLine(ProcessTags(summary));
        }

        public void WriteExample(string example)
        {
            if (example.IsNullOrEmpty()) return;

            WriteTitle(Options.Strings.Examples);
            WriteLine(ProcessTags(example));
        }
        public void WriteRemarks(string remarks)
        {
            if (remarks.IsNullOrEmpty()) return;

            WriteTitle(Options.Strings.Remarks);
            WriteLine(ProcessTags(remarks));
        }
        #endregion

        #region Low level write functions
        public void WriteBigTitle(string title) => OutputText.Append(Markdown.Heading(1, title));
        public void WriteTitle(string title) => OutputText.Append(Markdown.Heading(2, title));
        public void WriteSmallTitle(string title) => OutputText.Append(Markdown.Heading(3, title));
        public void WriteTableTitle(params string[] tableHeadings) => OutputText.Append(Markdown.TableTitle(tableHeadings));
        public void WriteTableRow(params string[] row) => OutputText.Append(Markdown.TableRow(row));
        public void WriteLine(string text) => OutputText.Append(Markdown.WithNewline(text));
        #endregion
    }
}
