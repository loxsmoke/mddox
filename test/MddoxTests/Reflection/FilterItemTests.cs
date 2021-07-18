using MdDox.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MddoxTests.Reflection
{
    public class FilterItemTests
    {
        [Theory]
        [InlineData("all.public", FilterType.All, FilterScope.Public, null)]
        [InlineData("type.protected", FilterType.Type, FilterScope.Protected, null)]
        [InlineData("property.private", FilterType.Property, FilterScope.Private, null)]
        [InlineData("field.attribute.Beer", FilterType.Field, FilterScope.Attribute, "BeerAttribute")]
        [InlineData("field.attribute.B?er*", FilterType.Field, FilterScope.Attribute, "B?er*")]
        [InlineData("method.name.Fake", FilterType.Method, FilterScope.Name, "Fake")]
        public void Parse(string text, FilterType expectedType, FilterScope expectedScope, string expectedParameter)
        {
            var result = FilterItem.Parse(text);
            Assert.Equal(expectedType, result.FilterType);
            Assert.Equal(expectedScope, result.FilterScope);
            Assert.Equal(expectedParameter, result.FilterParameter);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("nodot")]
        [InlineData("all.name")]
        [InlineData("all.attribute")]
        [InlineData("error.all")]
        [InlineData("all.error")]
        public void Parse_Error(string text)
        {
            Assert.Throws<ArgumentException>(() => FilterItem.Parse(text));
        }
    }
}
