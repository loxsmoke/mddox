using MdDox.Reflection;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MddoxTests.Reflection
{
    [TestClass]
    public class FilterItemTests
    {
        [DataTestMethod]
        [DataRow("all.public", FilterType.All, FilterScope.Public, null)]
        [DataRow("type.protected", FilterType.Type, FilterScope.Protected, null)]
        [DataRow("property.private", FilterType.Property, FilterScope.Private, null)]
        [DataRow("field.attribute.Beer", FilterType.Field, FilterScope.Attribute, "BeerAttribute")]
        [DataRow("field.attribute.B?er*", FilterType.Field, FilterScope.Attribute, "B?er*")]
        [DataRow("method.name.Fake", FilterType.Method, FilterScope.Name, "Fake")]
        [DataRow("all.inherited", FilterType.All, FilterScope.Inherited, null)]
        [DataRow("method.inherited", FilterType.Method, FilterScope.Inherited, null)]
        [DataRow("field.inherited", FilterType.Field, FilterScope.Inherited, null)]
        [DataRow("property.inherited", FilterType.Property, FilterScope.Inherited, null)]
        public void Parse(string text, FilterType expectedType, FilterScope expectedScope, string expectedParameter)
        {
            var result = FilterItem.Parse(text);
            Assert.AreEqual(expectedType, result.FilterType);
            Assert.AreEqual(expectedScope, result.FilterScope);
            Assert.AreEqual(expectedParameter, result.FilterParameter);
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "null")]
        [DataRow("", DisplayName = "empty")]
        [DataRow("nodot", DisplayName = "nodot")]
        [DataRow("all.name", DisplayName = "all.name")]
        [DataRow("all.attribute", DisplayName = "all.attribute")]
        [DataRow("error.all", DisplayName = "error.all")]
        [DataRow("all.error", DisplayName = "all.error")]
        [DataRow("type.inherited", DisplayName = "type.inherited")]
        public void Parse_Error(string text)
        {
            Assert.ThrowsException<ArgumentException>(() => FilterItem.Parse(text));
        }
    }
}
