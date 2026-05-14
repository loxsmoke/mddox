namespace MdDox.Reflection
{
    /// <summary>
    /// Defines the scope of the filter, such as visibility, presence of attribute, specific name, or inherited.
    /// </summary>
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
        Name,
        /// <summary>
        /// Filter applies to inherited fields, methods, or properties.
        /// </summary>
        Inherited
    }
}
