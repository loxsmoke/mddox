using System;
using System.Collections.Generic;
using System.Text;

namespace MdDox.Reflection
{
    public enum FilterScope
    {
        /// <summary>
        /// Filter applies to items with any visibility, names and attributes
        /// </summary>
        All,
        /// <summary>
        /// Filter applies only to public items
        /// </summary>
        Public,
        /// <summary>
        /// Filter applies only to protected items
        /// </summary>
        Protected,
        /// <summary>
        /// Filter applies only to private items
        /// </summary>
        Private,
        /// <summary>
        /// Filter applies only to items that have specified attribute
        /// </summary>
        Attribute,
        /// <summary>
        /// Filter applies only to items that have specified name wildcard
        /// </summary>
        Name
    }
}
