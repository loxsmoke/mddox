using MdDox.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

#pragma warning disable CS0414

namespace MddoxTests.Reflection
{
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

        [Theory]
        [InlineData(typeof(PublicClass), FilterScope.Public)]
        [InlineData(typeof(ProtectedClass), FilterScope.Protected)]
        [InlineData(typeof(PrivateClass), FilterScope.Private)]
        public void ToScope_Class(Type type, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type);
            Assert.Equal(expectedScope, result);
        }

        [Theory]
        [InlineData(typeof(PublicClass), "PublicField", FilterScope.Public)]
        [InlineData(typeof(PublicClass), "ProtectedField", FilterScope.Protected)]
        [InlineData(typeof(PublicClass), "PrivateField", FilterScope.Private)]
        public void ToScope_Field(Type type, string fieldName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.Equal(expectedScope, result);
        }

        [Theory]
        [InlineData(typeof(PublicClass), "PublicProperty", FilterScope.Public)]
        [InlineData(typeof(PublicClass), "PublicProperty2", FilterScope.Public)]
        [InlineData(typeof(PublicClass), "ProtectedProperty", FilterScope.Protected)]
        [InlineData(typeof(PublicClass), "PrivateProperty", FilterScope.Private)]
        public void ToScope_Property(Type type, string propertyName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetProperty(propertyName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.Equal(expectedScope, result);
        }

        [Theory]
        [InlineData(typeof(PublicClass), "PublicMethod", FilterScope.Public)]
        [InlineData(typeof(PublicClass), "ProtectedMethod", FilterScope.Protected)]
        [InlineData(typeof(PublicClass), "PrivateMethod", FilterScope.Private)]
        public void ToScope_Method(Type type, string fieldName, FilterScope expectedScope)
        {
            var result = TypeFilter.ToScope(type.GetMethod(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));
            Assert.Equal(expectedScope, result);
        }

        [Theory]
        [InlineData("","",true)]
        [InlineData("a","a",true)]
        [InlineData("name","name",true)]
        [InlineData("name1","name2",false)]
        [InlineData("name?","name2",true)]
        [InlineData("name*","name2",true)]
        [InlineData("name*","name2243232",true)]
        [InlineData("po*to","potato",true)]
        [InlineData("po*to","poto",true)]
        [InlineData("*a*b*c*","abc",true)]
        [InlineData("*a*b*c*","mmmmaddddddboooooocppp234",true)]
        public void MatchWildcard(string wildcard, string name, bool expectedResult)
        {
            var result = TypeFilter.MatchWildcard(wildcard, name);
            Assert.Equal(expectedResult, result);
        }
    }
}
