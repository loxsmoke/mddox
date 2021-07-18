using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MdDox.Reflection
{
    public class TypeFilter
    {
        public TypeFilterOptions Options { get; }
        public TypeFilter(TypeFilterOptions options)
        {
            Options = options;
        }

        public (bool match, FilterItem include, FilterItem exclude) MatchFilters(
            FilterType itemType, // Type of the item to check. Type, Method, Property, Field
            FilterScope scope,  // Scope of the item. Public, private, protected
            string name,  // Name of the item. Class name, method name, field name, etc.
            IEnumerable<Attribute> attributes)  // Custom attributes of the item
        {
            var include = MatchFilter(Options.Include, itemType, scope, name, attributes);
            if (include == null) return (false, null, null);
            var exclude = MatchFilter(Options.Exclude, itemType, scope, name, attributes);
            return (include != null && exclude == null, include, exclude);
        }

        public static FilterItem MatchFilter(
            List<FilterItem> filters,
            FilterType itemType, // Type of the item to check. Type, Method, Property, Field
            FilterScope scope,  // Scope of the item. Public, private, protected
            string name,  // Name of the item. Class name, method name, field name, etc.
            IEnumerable<Attribute> attributes)  // Custom attributes of the item
        {
            // No filters. Nothing goes through
            if (filters.Count == 0) return null;
            // Just one filter. Check if there is a special case when everything goes through
            if (filters.Count == 1 && filters.First().AllAllowed) return filters.First();

            // Check each filter item from the options
            foreach (var filterItem in filters.Where(it => it.FilterType == FilterType.All || it.FilterType == itemType))
            {
                // Check if there is a visibility match
                if (filterItem.FilterScope == FilterScope.All ||
                    filterItem.FilterScope == scope)
                    return filterItem;
                // Check if we have to match the name
                if (filterItem.FilterScope == FilterScope.Name &&
                    MatchWildcard(filterItem.FilterParameter, name))
                    return filterItem;
                // Check custom attributes
                if (filterItem.FilterScope == FilterScope.Attribute &&
                    attributes.Any() &&
                    attributes.Any(attr => attr.GetType().Name == filterItem.FilterParameter))
                    return filterItem;
            }
            return null;
        }

        public static bool MatchWildcard(string wildcard, string name)
        {
            if (name.IsNullOrEmpty() && wildcard.IsNullOrEmpty()) return true;
            if (name.IsNullOrEmpty() || wildcard.IsNullOrEmpty()) return false;
            if (wildcard == name) return true;
            return MatchWildcard(wildcard, 0, name, 0);
        }
        protected static bool MatchWildcard(string wildcard, int wi, string name, int i)
        {
            if (wi >= wildcard.Length) return (i >= name.Length);
            if (i >= name.Length) return wi == wildcard.Length - 1 && wildcard[wi] == '*';
            if (wildcard[wi] == '?' || wildcard[wi] == name[i]) return MatchWildcard(wildcard, wi + 1, name, i + 1);
            if (wildcard[wi] != '*') return false;
            if (wi == wildcard.Length - 1) return true;
            while (i < name.Length)
            {
                if (MatchWildcard(wildcard, wi + 1, name, i)) return true;
                i++;
            }
            return false;
        }

        #region Scope methods
        public static FilterScope ToScope(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic) return FilterScope.Public;
            if (type.IsNestedPrivate) return FilterScope.Private;
            return FilterScope.Protected;
        }
        public static FilterScope ToScope(FieldInfo field)
        {
            switch (field.Attributes & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.Private: 
                    return FilterScope.Private;
                case FieldAttributes.Public:
                    return FilterScope.Public;
            }
            return FilterScope.Protected;
        }
        public static FilterScope ToScope(PropertyInfo property)
        {
            var scope = FilterScope.Private;
            foreach (var accessor in property.GetAccessors(true))
            {
                var newScope = ToScope(accessor);
                if (scope == newScope) continue;

                if (scope == FilterScope.Private ||
                    newScope == FilterScope.Public)
                {
                    scope = newScope;
                }
            }
            return scope;
        }
        public static FilterScope ToScope(MethodBase method)
        {
            switch (method.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.PrivateScope:
                case MethodAttributes.Private:
                    return FilterScope.Private;
                case MethodAttributes.Public:
                    return FilterScope.Public;
            }
            return FilterScope.Protected;
        }
        #endregion

        #region Match methods
        public (bool match, FilterItem include, FilterItem exclude) MatchFilters(Type info)
        {
            return MatchFilters(FilterType.Type, ToScope(info), info.Name, info.GetCustomAttributes());
        }
        public (bool match, FilterItem include, FilterItem exclude) MatchFilters(FieldInfo info)
        {
            return MatchFilters(FilterType.Field, ToScope(info), info.Name, info.GetCustomAttributes());
        }
        public (bool match, FilterItem include, FilterItem exclude) MatchFilters(PropertyInfo info)
        {
            return MatchFilters(FilterType.Property, ToScope(info), info.Name, info.GetCustomAttributes());
        }
        public (bool match, FilterItem include, FilterItem exclude) MatchFilters(MethodBase info)
        {
            return MatchFilters(FilterType.Method, ToScope(info), info.Name, info.GetCustomAttributes());
        }
        #endregion
    }
}
