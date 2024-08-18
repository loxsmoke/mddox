using LoxSmoke.DocXml.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static DocXml.Reflection.ReflectionExtensions;

namespace MdDox.Reflection
{
    public class OrderedTypeList
    {
        public TypeCollection TypeCollection { get; set; }
        public List<TypeCollection.TypeInformation> TypesToDocument { get; set; }
        public HashSet<Type> TypesToDocumentSet { get; set; }

        public OrderedTypeList(TypeCollection typeCollection, Type firstType = null)
        {
            TypeCollection = typeCollection;
            TypesToDocument = typeCollection.ReferencedTypes.Values
                .OrderBy(t => t.Type.Namespace)
                .ThenBy(t => t.Type.Name).ToList();
            TypesToDocumentSet = new HashSet<Type>(TypesToDocument.Select(t => t.Type));

            if (firstType != null)
            {
                var typeDesc = TypesToDocument.FirstOrDefault(t => t.Type == firstType);
                if (typeDesc != null)
                {
                    TypesToDocument.Remove(typeDesc);
                    TypesToDocument.Insert(0, typeDesc);
                }
            }
        }

        public static OrderedTypeList LoadTypes(
            Type rootType,
            Assembly assembly,
            bool recursiveAssemblyTraversal,
            IEnumerable<string> recursiveAssemblies,
            TypeFilterOptions options,
            bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine("Include rules");
                if (!options.Include.Any()) Console.WriteLine("  NONE");
                foreach (var item in options.Include) Console.WriteLine("  " + item.ToString());
                Console.WriteLine("Exclude rules");
                if (!options.Exclude.Any()) Console.WriteLine("  NONE");
                foreach (var item in options.Exclude) Console.WriteLine("  " + item.ToString());
            }

            // Reflection setup
            var allAssemblyTypes = assembly != null;
            if (assembly == null) assembly = rootType.Assembly;
            var assemblyList = recursiveAssemblies.Any() ? recursiveAssemblies.ToList() : null;
            if (verbose && assembly != null) Log(assembly, "Root assembly ");
            var typeFilter = new TypeFilter(options);

            var reflectionSettings = ReflectionSettings.Default;
            reflectionSettings.FieldFilter = info => Filter(info, typeFilter, verbose);
            reflectionSettings.PropertyFilter = info => Filter(info, typeFilter, verbose);
            reflectionSettings.MethodFilter = info => Filter(info, typeFilter, verbose);
            reflectionSettings.TypeFilter = info => Filter(info, typeFilter, verbose);
            reflectionSettings.AssemblyFilter =
                reflectionAssembly => AssemblyFilter(reflectionAssembly, assembly, assemblyList, recursiveAssemblyTraversal, verbose);

            // Reflection
            var typeCollection = allAssemblyTypes ?
                TypeCollection.ForReferencedTypes(assembly, reflectionSettings) :
                TypeCollection.ForReferencedTypes(rootType, reflectionSettings);
            return new OrderedTypeList(typeCollection, rootType);
        }

        #region Filtering 
        public static bool Filter(Type info, TypeFilter filter, bool verbose)
        {
            var (document, include, exclude) = filter.MatchFilters(info);
            if (verbose)
            {
                Log(include, exclude, info, document);
            }
            return document;
        }
        public static bool Filter(FieldInfo info, TypeFilter filter, bool verbose)
        {
            var (document, include, exclude) = filter.MatchFilters(info);
            if (verbose)
            {
                Log(include, exclude, info, document);
            }
            return document;
        }
        public static bool Filter(PropertyInfo info, TypeFilter filter, bool verbose)
        {
            var (document, include, exclude) = filter.MatchFilters(info);
            if (verbose)
            {
                Log(include, exclude, info, document);
            }
            return document;
        }
        public static bool Filter(MethodBase info, TypeFilter filter, bool verbose)
        {
            var (document, include, exclude) = filter.MatchFilters(info);
            if (verbose)
            {
                Log(include, exclude, info, document);
            }
            return document;
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
        #endregion

        #region Logging
        public static void Log(FilterItem include, FilterItem exclude, Type type, bool document)
        {
            Console.WriteLine($"  {(document ? "Document" : "Ignore")} type {type.Namespace}.{type.ToNameString()} // {DecodeToString(type.Attributes)}");
            Console.WriteLine($"  Include rule: {include?.ToString() ?? "null"}  Exclude rule: {exclude?.ToString() ?? "null"}");
        }

        public static void Log(FilterItem include, FilterItem exclude, FieldInfo info, bool document)
        {
            Console.WriteLine($"    {(document ? "Document" : "Ignore")} field {info.ToTypeNameString()} {info.Name} // {info.MemberType} {DecodeToString(info.Attributes)}");
            Console.WriteLine($"    Include rule: {include?.ToString() ?? "null"}  Exclude rule: {exclude?.ToString() ?? "null"}");
        }

        public static void Log(FilterItem include, FilterItem exclude, PropertyInfo info, bool document)
        {
            Console.WriteLine($"    {(document ? "Document" : "Ignore")} property {info.ToTypeNameString()} {info.Name} // {info.MemberType} {DecodeToString(info.Attributes)}");
            Console.WriteLine($"    Include rule: {include?.ToString() ?? "null"}  Exclude rule: {exclude?.ToString() ?? "null"}");
            foreach (var acc in info.GetAccessors())
            {
                Console.WriteLine($"      Accessor {acc.Name}{acc.ToParametersString()} // {DecodeToString(acc.Attributes)}");
            }
        }

        public static void Log(FilterItem include, FilterItem exclude, MethodBase info, bool document)
        {
            Console.WriteLine($"    {(document ? "Document" : "Ignore")} method {info.Name}{info.ToParametersString()} // {DecodeToString(info.Attributes)}");
            Console.WriteLine($"    Include rule: {include?.ToString() ?? "null"}  Exclude rule: {exclude?.ToString() ?? "null"}");
        }

        public static void Log(Assembly assembly, string message)
        {
            Console.WriteLine(message + assembly.FullName);
            Console.WriteLine("File path: " + assembly.Location ?? "<empty>");
        }

        public static string DecodeToString(FieldAttributes attrs)
        {
            var text = string.Empty;
            switch (attrs & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.Private:
                    text = text.Add("Private");
                    break;
                case FieldAttributes.FamANDAssem:
                    text = text.Add("FamANDAssem");
                    break;
                case FieldAttributes.Assembly:
                    text = text.Add("Assembly");
                    break;
                case FieldAttributes.Family:
                    text = text.Add("Family");
                    break;
                case FieldAttributes.FamORAssem:
                    text = text.Add("FamORAssem");
                    break;
                case FieldAttributes.Public:
                    text = text.Add("Public");
                    break;
            }
            if (attrs.HasFlag(FieldAttributes.Static)) text = text.Add("Static");
            if (attrs.HasFlag(FieldAttributes.InitOnly)) text = text.Add("InitOnly");
            if (attrs.HasFlag(FieldAttributes.Literal)) text = text.Add("Literal");
            if (attrs.HasFlag(FieldAttributes.HasFieldRVA)) text = text.Add("HasFieldRVA");
            if (attrs.HasFlag(FieldAttributes.SpecialName)) text = text.Add("SpecialName");
            if (attrs.HasFlag(FieldAttributes.RTSpecialName)) text = text.Add("RTSpecialName");
            if (attrs.HasFlag(FieldAttributes.HasFieldMarshal)) text = text.Add("HasFieldMarshal");
            if (attrs.HasFlag(FieldAttributes.PinvokeImpl)) text = text.Add("PinvokeImpl");
            if (attrs.HasFlag(FieldAttributes.HasDefault)) text = text.Add("HasDefault");
            return text;
        }

        public static string DecodeToString(TypeAttributes attrs)
        {
            var text = string.Empty;
            switch (attrs & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.NotPublic:
                    text = text.Add("NotPublic");
                    break;
                case TypeAttributes.Public:
                    text = text.Add("Public");
                    break;
                case TypeAttributes.NestedPublic:
                    text = text.Add("NestedPublic");
                    break;
                case TypeAttributes.NestedPrivate:
                    text = text.Add("NestedPrivate");
                    break;
                case TypeAttributes.NestedFamANDAssem:
                    text = text.Add("NestedFamANDAssem");
                    break;
                case TypeAttributes.NestedAssembly:
                    text = text.Add("NestedAssembly");
                    break;
                case TypeAttributes.NestedFamily:
                    text = text.Add("NestedFamily"); // nested and protected
                    break;
                case TypeAttributes.NestedFamORAssem:
                    text = text.Add("NestedFamORAssem"); // nested and protected internal
                    break;
            }

            // Use the layout mask to test for layout attributes.
            switch (attrs & TypeAttributes.LayoutMask)
            {
                case TypeAttributes.AutoLayout:
                    text = text.Add("AutoLayout");
                    break;
                case TypeAttributes.SequentialLayout:
                    text = text.Add("SequentialLayout");
                    break;
                case TypeAttributes.ExplicitLayout:
                    text = text.Add("ExplicitLayout");
                    break;
            }

            // Use the class semantics mask to test for class semantics attributes.
            switch (attrs & TypeAttributes.ClassSemanticsMask)
            {
                case TypeAttributes.Class:
                    text = text.Add("Class");
                    break;
                case TypeAttributes.Interface:
                    text = text.Add("Interface");
                    break;
            }

            if ((attrs & TypeAttributes.Abstract) != 0)
            {
                text = text.Add("Abstract");
            }

            if ((attrs & TypeAttributes.Sealed) != 0)
            {
                text = text.Add("Sealed");
            }
            return text;
        }

        public static string DecodeToString(MethodAttributes attrs)
        {
            var text = string.Empty;
            switch (attrs & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.PrivateScope:
                    text = text.Add("PrivateScope");
                    text = text.Add("ReuseSlot");
                    break;
                case MethodAttributes.Private:
                    text = text.Add("Private");
                    break;
                case MethodAttributes.FamANDAssem:
                    text = text.Add("FamANDAssem");
                    break;
                case MethodAttributes.Assembly:
                    text = text.Add("Assembly");
                    break;
                case MethodAttributes.Family:
                    text = text.Add("Family");
                    break;
                case MethodAttributes.FamORAssem:
                    text = text.Add("FamORAssem");
                    break;
                case MethodAttributes.Public:
                    text = text.Add("Public");
                    break;
            }
            if (attrs.HasFlag(MethodAttributes.UnmanagedExport)) text = text.Add("UnmanagedExport");
            if (attrs.HasFlag(MethodAttributes.Static)) text = text.Add("Static");
            if (attrs.HasFlag(MethodAttributes.Final)) text = text.Add("Final");
            if (attrs.HasFlag(MethodAttributes.Virtual)) text = text.Add("Virtual");
            if (attrs.HasFlag(MethodAttributes.HideBySig)) text = text.Add("HideBySig");
            if (attrs.HasFlag(MethodAttributes.NewSlot)) text = text.Add("NewSlot");
            if (attrs.HasFlag(MethodAttributes.VtableLayoutMask)) text = text.Add("VtableLayoutMask");
            if (attrs.HasFlag(MethodAttributes.CheckAccessOnOverride)) text = text.Add("CheckAccessOnOverride");
            if (attrs.HasFlag(MethodAttributes.Abstract)) text = text.Add("Abstract");
            if (attrs.HasFlag(MethodAttributes.SpecialName)) text = text.Add("SpecialName");
            if (attrs.HasFlag(MethodAttributes.RTSpecialName)) text = text.Add("RTSpecialName");
            if (attrs.HasFlag(MethodAttributes.PinvokeImpl)) text = text.Add("PinvokeImpl");
            if (attrs.HasFlag(MethodAttributes.HasSecurity)) text = text.Add("HasSecurity");
            if (attrs.HasFlag(MethodAttributes.RequireSecObject)) text = text.Add("RequireSecObject");
            return text;
        }
        public static string DecodeToString(PropertyAttributes attrs)
        {
            var text = string.Empty;
            if (attrs.HasFlag(PropertyAttributes.HasDefault)) text = text.Add("HasDefault");
            if (attrs.HasFlag(PropertyAttributes.ReservedMask)) text = text.Add("ReservedMask");
            if (attrs.HasFlag(PropertyAttributes.RTSpecialName)) text = text.Add("RTSpecialName");
            if (attrs.HasFlag(PropertyAttributes.SpecialName)) text = text.Add("SpecialName");
            return text;
        }
        #endregion
    }
}
