using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdDox.Reflection
{
    public class TypeFilterOptions
    {
        /// <summary>
        /// Which items to include. This condition is applied first
        /// </summary>
        public List<FilterItem> Include = new List<FilterItem>() { FilterItem.IncludeAll };
        /// <summary>
        /// Which items to exclude. This condition is applied after the Include
        /// </summary>
        public List<FilterItem> Exclude = new List<FilterItem>();

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
                Exclude = exclude.Select(filter => FilterItem.Parse(filter)).ToList()
            };
            if (include.Any())
            {
                options.Include = include.Select(filter => FilterItem.Parse(filter)).ToList();
            }
            return options;
        }
    }
}
