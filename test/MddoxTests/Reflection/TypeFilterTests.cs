using MdDox.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS0414

namespace MddoxTests.Reflection
{
    [TestClass]
    public class TypeFilterTests
    {
        #region Test data
        public class PublicClass
        {
            public int PublicField;
            protected int ProtectedField;
            private int PrivateField;

            public int PublicProperty { get; set; }
            public int PublicProperty2 { get; protected set; }
            protected int ProtectedProperty { get; set; }
            private int PrivateProperty { get; set; }

            public void PublicMethod() { PrivateField = 1; }
            protected void ProtectedMethod() { }
            private void PrivateMethod() { }
        }
        protected class ProtectedClass
        {
        }
        private class PrivateClass
        {
        }
        #endregion

        [DataTestMethod]
        [DataRow(typeof(PublicClass), FilterScope.Public)]
        [DataRow(typeof(ProtectedClass), FilterScope.Protected)]
        [DataRow(typeof(PrivateClass), FilterScope.Private)]
        public void ToScope_Class(Type type, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type);
            Assert.AreEqual(expectedScope, result);
        }

        [DataTestMethod]
        [DataRow(typeof(PublicClass), "PublicField", FilterScope.Public)]
        [DataRow(typeof(PublicClass), "ProtectedField", FilterScope.Protected)]
        [DataRow(typeof(PublicClass), "PrivateField", FilterScope.Private)]
        public void ToScope_Field(Type type, string fieldName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.AreEqual(expectedScope, result);
        }

        [DataTestMethod]
        [DataRow(typeof(PublicClass), "PublicProperty", FilterScope.Public)]
        [DataRow(typeof(PublicClass), "PublicProperty2", FilterScope.Public)]
        [DataRow(typeof(PublicClass), "ProtectedProperty", FilterScope.Protected)]
        [DataRow(typeof(PublicClass), "PrivateProperty", FilterScope.Private)]
        public void ToScope_Property(Type type, string propertyName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetProperty(propertyName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.AreEqual(expectedScope, result);
        }

        [DataTestMethod]
        [DataRow(typeof(PublicClass), "PublicMethod", FilterScope.Public)]
        [DataRow(typeof(PublicClass), "ProtectedMethod", FilterScope.Protected)]
        [DataRow(typeof(PublicClass), "PrivateMethod", FilterScope.Private)]
        public void ToScope_Method(Type type, string fieldName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetMethod(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.AreEqual(expectedScope, result);
        }

        [DataTestMethod]
        [DataRow("","",true)]
        [DataRow("a","a",true)]
        [DataRow("name","name",true)]
        [DataRow("name1","name2",false)]
        [DataRow("name?","name2",true)]
        [DataRow("name*","name2",true)]
        [DataRow("name*","name2243232",true)]
        [DataRow("po*to","potato",true)]
        [DataRow("po*to","poto",true)]
        [DataRow("*a*b*c*","abc",true)]
        [DataRow("*a*b*c*","mmmmaddddddboooooocppp234",true)]
        public void MatchWildcard(string wildcard, string name, bool expectedResult)
        {
            var result = TypeFilter.MatchWildcard(wildcard, name);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void IncludeInherited_NoIncludeOrExclude_ReturnsFalse()
        {
            var options = new TypeFilterOptions
            {
                Include = [],
                Exclude = []
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
            Assert.IsFalse(options.IncludeInherited(FilterType.Property));
            Assert.IsFalse(options.IncludeInherited(FilterType.Field));
        }

        [TestMethod]
        public void IncludeInherited_IncludeInheritedMethod_ReturnsTrue()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited }
                ],
                Exclude = []
            };

            Assert.IsTrue(options.IncludeInherited(FilterType.Method));
            Assert.IsFalse(options.IncludeInherited(FilterType.Property));
            Assert.IsFalse(options.IncludeInherited(FilterType.Field));
        }

        [TestMethod]
        public void IncludeInherited_IncludeInheritedAll_ReturnsTrueForAllTypes()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.All, FilterScope = FilterScope.Inherited }
                ],
                Exclude = []
            };

            Assert.IsTrue(options.IncludeInherited(FilterType.Method));
            Assert.IsTrue(options.IncludeInherited(FilterType.Property));
            Assert.IsTrue(options.IncludeInherited(FilterType.Field));
            Assert.IsTrue(options.IncludeInherited(FilterType.Type));
        }

        [TestMethod]
        public void IncludeInherited_IncludeAndExcludeMethod_ReturnsFalse()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited }
                ],
                Exclude =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited }
                ]
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
        }

        [TestMethod]
        public void IncludeInherited_IncludeAllExcludeMethod_ReturnsFalseForMethod()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.All, FilterScope = FilterScope.Inherited }
                ],
                Exclude =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited }
                ]
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
            Assert.IsTrue(options.IncludeInherited(FilterType.Property));
            Assert.IsTrue(options.IncludeInherited(FilterType.Field));
        }

        [TestMethod]
        public void IncludeInherited_IncludeMethodExcludeAll_ReturnsFalseForMethod()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited }
                ],
                Exclude =
                [
                    new FilterItem { FilterType = FilterType.All, FilterScope = FilterScope.Inherited }
                ]
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
        }

        [TestMethod]
        public void IncludeInherited_NonInheritedScope_ReturnsFalse()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Public }
                ],
                Exclude = []
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
        }

        [TestMethod]
        public void IncludeInherited_MultipleIncludeFilters_ReturnsTrue()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Public },
                    new FilterItem { FilterType = FilterType.Property, FilterScope = FilterScope.Inherited }
                ],
                Exclude = []
            };

            Assert.IsFalse(options.IncludeInherited(FilterType.Method));
            Assert.IsTrue(options.IncludeInherited(FilterType.Property));
        }

        [TestMethod]
        public void IncludeInherited_MultipleFiltersWithMatching_ReturnsCorrectly()
        {
            var options = new TypeFilterOptions
            {
                Include =
                [
                    new FilterItem { FilterType = FilterType.Method, FilterScope = FilterScope.Inherited },
                    new FilterItem { FilterType = FilterType.Property, FilterScope = FilterScope.Inherited },
                    new FilterItem { FilterType = FilterType.Field, FilterScope = FilterScope.Public }
                ],
                Exclude =
                [
                    new FilterItem { FilterType = FilterType.Property, FilterScope = FilterScope.Inherited }
                ]
            };

            Assert.IsTrue(options.IncludeInherited(FilterType.Method));
            Assert.IsFalse(options.IncludeInherited(FilterType.Property));
            Assert.IsFalse(options.IncludeInherited(FilterType.Field));
        }
    }
}
