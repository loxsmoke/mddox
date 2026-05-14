using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdDox.Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MddoxTests.Localization
{
    [TestClass]
    public class LocalizedStringsTests
    {
        private static IEnumerable<Type> GetLocalizedStringTypes()
        {
            var assembly = typeof(ILocalizedStrings).Assembly;
            return assembly.GetTypes()
                .Where(t => typeof(ILocalizedStrings).IsAssignableFrom(t) 
                    && !t.IsInterface 
                    && !t.IsAbstract);
        }

        [TestMethod]
        public void AllPropertiesReturnNonEmptyValues()
        {
            // Arrange
            var localizationTypes = GetLocalizedStringTypes().ToList();
            Assert.IsTrue(localizationTypes.Count > 0, 
                "No types implementing ILocalizedStrings were found");

            var interfaceType = typeof(ILocalizedStrings);
            var properties = interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Act & Assert
            foreach (var localizationType in localizationTypes)
            {
                var instance = Activator.CreateInstance(localizationType) as ILocalizedStrings;
                Assert.IsNotNull(instance, $"Failed to create instance of {localizationType.Name}");

                foreach (var property in properties)
                {
                    var value = property.GetValue(instance) as string;
                    
                    Assert.IsNotNull(value, 
                        $"Property {property.Name} returned null in {localizationType.Name}");
                    
                    Assert.IsFalse(string.IsNullOrWhiteSpace(value), 
                        $"Property {property.Name} returned empty or whitespace value in {localizationType.Name}");
                }
            }
        }

        [TestMethod]
        public void CultureName_IsUnique()
        {
            // Arrange
            var localizationTypes = GetLocalizedStringTypes().ToList();
            Assert.IsTrue(localizationTypes.Count > 0, 
                "No types implementing ILocalizedStrings were found");

            var cultureNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Act & Assert
            foreach (var localizationType in localizationTypes)
            {
                var instance = Activator.CreateInstance(localizationType) as ILocalizedStrings;
                var cultureName = instance.CultureName;

                Assert.IsFalse(cultureNames.Contains(cultureName),
                    $"Duplicate culture name '{cultureName}' found in {localizationType.Name}");

                cultureNames.Add(cultureName);
            }
        }
    }
}
