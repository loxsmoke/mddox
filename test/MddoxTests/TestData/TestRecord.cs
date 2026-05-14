namespace MddoxTests.TestData
{
    /// <summary>
    /// A test record for documentation generation testing.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <param name="Name">The name.</param>
    public record TestRecord(int Id, string Name)
    {
        /// <summary>
        /// Gets a formatted display string.
        /// </summary>
        /// <returns>The formatted string.</returns>
        public string GetDisplay()
        {
            return $"{Id}: {Name}";
        }
    }
}
