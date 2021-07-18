using System;
using System.Collections.Generic;
using System.Text;

namespace MdDox.Reflection
{
    public enum FilterType
    {
        /// <summary>
        /// Filter applies to all items
        /// </summary>
        All,
        /// <summary>
        /// Filter applies only to type definitions like classes, structs, enums
        /// </summary>
        Type,
        /// <summary>
        /// Filter applies only to properties
        /// </summary>
        Property,
        /// <summary>
        /// Filter applies only to fields
        /// </summary>
        Field,
        /// <summary>
        /// Filter applies only to methods
        /// </summary>
        Method 
    }
}
