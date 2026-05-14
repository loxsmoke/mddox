using System.Collections.Generic;
using System.Linq;

namespace MdDox.Reflection
{
    /// <summary>
    /// Full type filtering options, including both include and exclude filters. 
    /// </summary>
    public class TypeFilterOptions
    {
        /// <summary>
        /// Which items to include. This condition is applied first
        /// </summary>
        public List<FilterItem> Include = [ FilterItem.IncludeAll ];

        /// <summary>
        /// Which items to exclude. This condition is applied after the Include
        /// </summary>
        public List<FilterItem> Exclude = [];

        /// <summary>
        /// Parse the options. Assume that "all" items are included if include enumeration is empty.
        /// </summary>
        /// <param name="include">Include filters</param>
        /// <param name="exclude">Exclude filters</param>
        /// <returns></returns>
        public static TypeFilterOptions Parse(IEnumerable<string> include, IEnumerable<string> exclude)
        {
            var options = new TypeFilterOptions()
            {
                Exclude = exclude.Select(FilterItem.Parse).ToList()
            };
            if (include.Any())
            {
                options.Include = include.Select(FilterItem.Parse).ToList();
            }

            // Default inheritance visibility setup. Properties were always included so make sure that they still
            // are included even if not specified
            if (!options.Include.Any(s => s.FilterScope == FilterScope.Inherited &&
                (s.FilterType == FilterType.Property || s.FilterType == FilterType.All)) &&
                !options.Exclude.Any(s => s.FilterScope == FilterScope.Inherited &&
                (s.FilterType == FilterType.Property || s.FilterType == FilterType.All)))
            {
                options.Include.Add(new() { FilterType = FilterType.Property, FilterScope = FilterScope.Inherited });
            }

            return options;
        }

        /// <summary>
        /// Return true if inherited class, property, field or method should be included based on the filter options.
        /// </summary>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public bool IncludeInherited(FilterType filterType)
        {
            bool include = Include.Any(s => s.FilterScope == FilterScope.Inherited &&
                (s.FilterType == filterType || s.FilterType == FilterType.All));
            bool exclude = Exclude.Any(s => s.FilterScope == FilterScope.Inherited &&
                (s.FilterType == filterType || s.FilterType == FilterType.All));
            return include && !exclude;
        }
    }
}
