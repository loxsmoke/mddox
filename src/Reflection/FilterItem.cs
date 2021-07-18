using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdDox.Reflection
{
    public class FilterItem
    {
        /// <summary>
        /// All, type, method or property
        /// </summary>
        public FilterType FilterType;
        /// <summary>
        /// All, certain visibilty like public or protected, presence of attribute or specific name
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
        public static FilterItem IncludeAll => new FilterItem() { FilterType = FilterType.All, FilterScope = FilterScope.All };

        public override string ToString()
        {
            return $"{FilterType}.{FilterScope}".Add(FilterParameter, ".");
        }

        /// <summary>
        /// Parse the filter string and return filter item object. Use case-insensitive enum values to parse the
        /// filter type and scope values.
        /// May throw ArgumentException if filter is empty or invalid.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Parsed filter item.</returns>
        public static FilterItem Parse(string text)
        {
            if (text.IsNullOrEmpty() || !text.Contains('.')) throw new ArgumentException($"Invalid filter item \"{text}\" format");
            var item = new FilterItem();
            var i = text.IndexOf('.');
            try
            {
                item.FilterType = (FilterType)Enum.Parse(typeof(FilterType), text.Substring(0, i), true);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid filter type value \"{text}\"");
            }
            var j = text.IndexOf('.', ++i);
            try
            {
                item.FilterScope = (FilterScope)Enum.Parse(typeof(FilterScope), j < 0 ? text[i..] : text[i..j], true);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid filter scope value \"{text}\"");
            }
        
            if (j > 0) // Filter parameter is the rest of the string
            {
                item.FilterParameter = text[(j + 1)..].DedupChar('*');
            }
            if ((item.FilterScope == FilterScope.Attribute ||
                item.FilterScope == FilterScope.Name) &&
                item.FilterParameter.IsNullOrEmpty())
            {
                throw new ArgumentException($"Missing filter name parameter \"{text}\"");
            }
            if (item.FilterScope == FilterScope.Attribute &&
                item.FilterParameter.IndexOfAny(new char[] { '?', '^' }) < 0 && 
                !item.FilterParameter.EndsWith("Attribute"))
            {
                item.FilterParameter += "Attribute";
            }
            return item;
        }
    }
}
