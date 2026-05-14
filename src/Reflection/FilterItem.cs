using System;

namespace MdDox.Reflection
{
    /// <summary>
    /// Filter item used for reflection-based filtering.
    /// </summary>
    public class FilterItem
    {
        /// <summary>
        /// All, type, method or property
        /// </summary>
        public FilterType FilterType;
        /// <summary>
        /// All, certain visibilty like public or protected, presence of attribute, specific name, or inherited
        /// </summary>
        public FilterScope FilterScope;
        /// <summary>
        /// Attribute or name value
        /// </summary>
        public string FilterParameter;

        /// <summary>
        /// Check if filter item is applicable to anything. Passthrough filter.
        /// </summary>
        public bool AllAllowed => FilterType == FilterType.All && FilterScope == FilterScope.All;

        /// <summary>
        /// Filter that lets everything through.
        /// </summary>
        public static FilterItem IncludeAll => new() { FilterType = FilterType.All, FilterScope = FilterScope.All };

        /// <inheritdoc/>
        public override string ToString() => $"{FilterType}.{FilterScope}".Add(FilterParameter, ".");

        /// <summary>
        /// Parse the filter string and return filter item object. Use case-insensitive enum values to parse the
        /// filter type and scope values.
        /// May throw ArgumentException if filter is empty or invalid.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Parsed filter item.</returns>
        public static FilterItem Parse(string text)
        {
            if (text.IsNullOrEmpty() || !text.Contains('.')) throw new ArgumentException($"Invalid filter item {text.Quoted()} format");

            var parts = text.Split('.');
            if (parts.Length < 2) throw new ArgumentException($"Invalid filter item {text.Quoted()} format");

            var item = new FilterItem();
            try
            {
                item.FilterType = Enum.Parse<FilterType>(parts[0], true);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid filter type value {parts[0]} in {text.Quoted()}");
            }

            try
            {
                item.FilterScope = Enum.Parse<FilterScope>(parts[1], true);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid filter scope value {parts[1]} in {text.Quoted()}");
            }

            if (parts.Length > 2) // Filter parameter is the rest joined by '.'
            {
                item.FilterParameter = string.Join('.', parts[2..]).DedupChar('*');
            }

            if ((item.FilterScope == FilterScope.Attribute ||
                item.FilterScope == FilterScope.Name) &&
                item.FilterParameter.IsNullOrEmpty())
            {
                throw new ArgumentException($"Missing filter name parameter {text.Quoted()}");
            }

            if (item.FilterScope == FilterScope.Attribute &&
                item.FilterParameter.IndexOfAny(['?', '^']) < 0 && 
                !item.FilterParameter.EndsWith("Attribute"))
            {
                item.FilterParameter += "Attribute";
            }

            if (item.FilterScope == FilterScope.Inherited &&
                item.FilterType != FilterType.All &&
                item.FilterType != FilterType.Method &&
                item.FilterType != FilterType.Field &&
                item.FilterType != FilterType.Property)
            {
                throw new ArgumentException($"Inherited filter scope only supports All, Method, Field, or Property filter types in {text.Quoted()}");
            }

            return item;
        }
    }
}
