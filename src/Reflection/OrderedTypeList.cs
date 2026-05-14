using LoxSmoke.DocXml.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using static DocXml.Reflection.ReflectionExtensions;

namespace MdDox.Reflection
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class OrderedTypeList
    {
        public TypeCollection TypeCollection { get; set; }
        /// <summary>
        /// Ordered list of types to document. The order is determined by the order in which the types are referenced in the XML documentation file,
        /// with the option to specify a first type to document.
        /// </summary>
        public List<TypeCollection.TypeInformation> TypesToDocument { get; set; }
        /// <summary>
        /// Hash set of types to document for quick lookup when processing members. 
        /// This is used to determine if a member's type should be documented without having to scan the TypesToDocument list.
        /// </summary>
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
            reflectionSettings.PropertyFlags = typeFilter.PropertyFlags;
            reflectionSettings.MethodFlags = typeFilter.MethodFlags;
            reflectionSettings.FieldFlags = typeFilter.FieldFlags;

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

            if (!recursiveAssemblies.Any(name => name.EqualsIgnoreCase(Path.GetFileName(assembly.Location))))
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

        /// <summary>
        /// Decodes the specified set of field attribute flags into a space-separated string,
        /// with the access level flag first followed by any additional flags.
        /// </summary>
        /// <param name="attrs"></param>
        /// <returns>A space separated string of flags and values.</returns>
        public static string DecodeToString(FieldAttributes attrs)
        {
            var flags = EnumFlags(attrs & ~FieldAttributes.FieldAccessMask);
            return string.Join(" ", flags.Prepend((attrs & FieldAttributes.FieldAccessMask).ToString()));
        }

        /// <summary>
        /// Decodes the specified set of type attribute flags into a space-separated string.
        /// </summary>
        /// <param name="attrs"></param>
        /// <returns>A space separated string of flags and values.</returns>
        public static string DecodeToString(TypeAttributes attrs)
        {
            List<string> values = [
                EnumValues(attrs & TypeAttributes.VisibilityMask, 
                    (TypeAttributes.NotPublic, nameof(TypeAttributes.NotPublic))),
                EnumValues(attrs & TypeAttributes.LayoutMask, 
                    (TypeAttributes.AutoLayout, nameof(TypeAttributes.AutoLayout))),
                EnumValues(attrs & TypeAttributes.ClassSemanticsMask, 
                    (TypeAttributes.Class, nameof(TypeAttributes.Class)),
                    (TypeAttributes.Interface, nameof(TypeAttributes.Interface)))];

            values.AddRange(EnumFlags(attrs, TypeAttributes.Abstract, TypeAttributes.Sealed));
            return string.Join(" ", values);
        }

        /// <summary>
        /// Decodes the specified set of method attribute flags into a space-separated string.
        /// </summary>
        /// <remarks>The returned string omits the VtableLayoutMask flag and includes only relevant
        /// MethodAttributes flags.</remarks>
        /// <param name="attrs"></param>
        /// <returns>A space-separated string containing the names of the decoded method attribute flags.</returns>
        public static string DecodeToString(MethodAttributes attrs)
        {
            // Ignore VtableLayoutMask
            List<string> values = [
                EnumValues(attrs & MethodAttributes.MemberAccessMask, 
                (MethodAttributes.PrivateScope, nameof(MethodAttributes.PrivateScope)))];
            
            values.AddRange(EnumFlags(attrs,
                MethodAttributes.UnmanagedExport,
                MethodAttributes.Static,
                MethodAttributes.Final,
                MethodAttributes.Virtual,
                MethodAttributes.HideBySig,
                MethodAttributes.CheckAccessOnOverride,
                MethodAttributes.Abstract,
                MethodAttributes.SpecialName,
                MethodAttributes.RTSpecialName,
                MethodAttributes.PinvokeImpl,
                MethodAttributes.HasSecurity,
                MethodAttributes.RequireSecObject));
            return string.Join(" ", values);
        }

        /// <summary>
        /// Decode property attributes. Do not include masks or reserved values
        /// </summary>
        /// <param name="attrs"></param>
        /// <returns>List of property attribute names</returns>
        public static string DecodeToString(PropertyAttributes attrs)
        {
            return string.Join(" ", EnumFlags(attrs, 
                PropertyAttributes.HasDefault, 
                PropertyAttributes.RTSpecialName, 
                PropertyAttributes.SpecialName));
        }

        #region Generic enum flag and value conversions
        /// <summary>
        /// Splits an enum value into its individual flag components.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The combined enum value</param>
        /// <returns>A list of individual enum flags that are set in the value</returns>
        public static List<string> EnumFlags<T>(T value) where T : Enum
        {
            var result = new List<string>();
            foreach (T flag in Enum.GetValues(typeof(T)))
            {
                if (!flag.Equals(default(T)) && value.HasFlag(flag))
                {
                    // Check if this is a single-bit flag. Multiple bits are masks
                    var flagValue = Convert.ToUInt64(flag);
                    if (BitOperations.PopCount(flagValue) == 1)
                    {
                        result.Add(flag.ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Return the list of flags in enum value. Check only specified flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to check</param>
        /// <param name="flags">The list of enum values to check against the value</param>
        /// <returns>A list of flag names that are set in the value</returns>
        public static List<string> EnumFlags<T>(T value, params T[] flags) where T : Enum
        {
            return flags.Where(flag => value.HasFlag(flag)).Select(flag => flag.ToString()).ToList();
        }

        /// <summary>
        /// Return the string representation of an enum value based on a value map.
        /// This method is used for enums that use the same value for multiple named constants, 
        /// such as the visibility and layout masks in TypeAttributes.
        /// When not specified enum.ToString() returns the first matching name which may not be the most descriptive one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to check</param>
        /// <param name="valueMap">The map of special values to their string representations</param>
        /// <returns>The string representation of the enum value</returns>
        public static string EnumValues<T>(T value,
            params (T enumValue, string enumText)[] valueMap) where T : Enum
        {
            return valueMap.FirstOrDefault(k => value.Equals(k.Item1), (value, enumText: value.ToString()))
                .enumText;
        }
        #endregion
        #endregion
    }
}
