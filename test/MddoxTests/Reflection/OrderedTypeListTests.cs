using MdDox.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace MddoxTests.Reflection
{
    [TestClass]
    public class OrderedTypeListTests
    {
        #region FieldAttributes Tests

        [DataTestMethod]
        [DataRow(FieldAttributes.Private, "Private", DisplayName = "Private")]
        [DataRow(FieldAttributes.Public, "Public", DisplayName = "Public")]
        [DataRow(FieldAttributes.FamANDAssem, "FamANDAssem", DisplayName = "FamANDAssem")]
        [DataRow(FieldAttributes.Assembly, "Assembly", DisplayName = "Assembly")]
        [DataRow(FieldAttributes.Family, "Family", DisplayName = "Family")]
        [DataRow(FieldAttributes.FamORAssem, "FamORAssem", DisplayName = "FamORAssem")]
        public void DecodeToString_FieldAttributes_AccessModifiers(FieldAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(FieldAttributes.Public | FieldAttributes.Static, "Public Static", DisplayName = "PublicStatic")]
        [DataRow(FieldAttributes.Public | FieldAttributes.InitOnly, "Public InitOnly", DisplayName = "PublicInitOnly")]
        [DataRow(FieldAttributes.Public | FieldAttributes.Literal, "Public Literal", DisplayName = "PublicLiteral")]
        [DataRow(FieldAttributes.Public | FieldAttributes.HasFieldRVA, "Public HasFieldRVA", DisplayName = "PublicHasFieldRVA")]
        [DataRow(FieldAttributes.Public | FieldAttributes.SpecialName, "Public SpecialName", DisplayName = "PublicSpecialName")]
        [DataRow(FieldAttributes.Public | FieldAttributes.RTSpecialName, "Public RTSpecialName", DisplayName = "PublicRTSpecialName")]
        [DataRow(FieldAttributes.Public | FieldAttributes.HasFieldMarshal, "Public HasFieldMarshal", DisplayName = "PublicHasFieldMarshal")]
        [DataRow(FieldAttributes.Public | FieldAttributes.PinvokeImpl, "Public PinvokeImpl", DisplayName = "PublicPinvokeImpl")]
        [DataRow(FieldAttributes.Public | FieldAttributes.HasDefault, "Public HasDefault", DisplayName = "PublicHasDefault")]
        [DataRow(FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, "Public Static InitOnly", DisplayName = "PublicStaticInitOnly")]
        [DataRow(FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.SpecialName, "Public Static InitOnly SpecialName", DisplayName = "ComplexCombination")]
        public void DecodeToString_FieldAttributes_Combinations(FieldAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region TypeAttributes Tests

        [DataTestMethod]
        [DataRow(TypeAttributes.NotPublic, "NotPublic AutoLayout Class", DisplayName = "NotPublic")]
        [DataRow(TypeAttributes.Public, "Public AutoLayout Class", DisplayName = "Public")]
        [DataRow(TypeAttributes.NestedPublic, "NestedPublic AutoLayout Class", DisplayName = "NestedPublic")]
        [DataRow(TypeAttributes.NestedPrivate, "NestedPrivate AutoLayout Class", DisplayName = "NestedPrivate")]
        [DataRow(TypeAttributes.NestedFamANDAssem, "NestedFamANDAssem AutoLayout Class", DisplayName = "NestedFamANDAssem")]
        [DataRow(TypeAttributes.NestedAssembly, "NestedAssembly AutoLayout Class", DisplayName = "NestedAssembly")]
        [DataRow(TypeAttributes.NestedFamily, "NestedFamily AutoLayout Class", DisplayName = "NestedFamily")]
        [DataRow(TypeAttributes.NestedFamORAssem, "NestedFamORAssem AutoLayout Class", DisplayName = "NestedFamORAssem")]
        public void DecodeToString_TypeAttributes_Visibility(TypeAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(TypeAttributes.AutoLayout, "NotPublic AutoLayout Class", DisplayName = "AutoLayout")]
        [DataRow(TypeAttributes.SequentialLayout, "NotPublic SequentialLayout Class", DisplayName = "SequentialLayout")]
        [DataRow(TypeAttributes.ExplicitLayout, "NotPublic ExplicitLayout Class", DisplayName = "ExplicitLayout")]
        [DataRow(TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Class, "Public SequentialLayout Class", DisplayName = "PublicSequentialLayoutClass")]
        [DataRow(TypeAttributes.Public | TypeAttributes.ExplicitLayout | TypeAttributes.Class, "Public ExplicitLayout Class", DisplayName = "PublicExplicitLayoutClass")]
        public void DecodeToString_TypeAttributes_Layout(TypeAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(TypeAttributes.Class, "NotPublic AutoLayout Class", DisplayName = "Class")]
        [DataRow(TypeAttributes.Interface, "NotPublic AutoLayout Interface", DisplayName = "Interface")]
        [DataRow(TypeAttributes.Public | TypeAttributes.Class, "Public AutoLayout Class", DisplayName = "PublicClass")]
        [DataRow(TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract, "Public AutoLayout Class Abstract", DisplayName = "PublicAbstractClass")]
        [DataRow(TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, "Public AutoLayout Class Sealed", DisplayName = "PublicSealedClass")]
        [DataRow(TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, "Public AutoLayout Class Abstract Sealed", DisplayName = "PublicAbstractSealedClass")]
        [DataRow(TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract, "Public AutoLayout Interface Abstract", DisplayName = "PublicInterfaceAbstract")]
        public void DecodeToString_TypeAttributes_ClassSemantics(TypeAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region MethodAttributes Tests

        [TestMethod]
        public void DecodeToString_MethodAttributes_PrivateScope()
        {
            var result = OrderedTypeList.DecodeToString(MethodAttributes.PrivateScope);
            Assert.AreEqual("PrivateScope", result);
        }

        [DataTestMethod]
        [DataRow(MethodAttributes.Private, "Private", DisplayName = "Private")]
        [DataRow(MethodAttributes.FamANDAssem, "FamANDAssem", DisplayName = "FamANDAssem")]
        [DataRow(MethodAttributes.Assembly, "Assembly", DisplayName = "Assembly")]
        [DataRow(MethodAttributes.Family, "Family", DisplayName = "Family")]
        [DataRow(MethodAttributes.FamORAssem, "FamORAssem", DisplayName = "FamORAssem")]
        [DataRow(MethodAttributes.Public, "Public", DisplayName = "Public")]
        public void DecodeToString_MethodAttributes_AccessModifiers(MethodAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(MethodAttributes.Public | MethodAttributes.Static, "Public Static", DisplayName = "PublicStatic")]
        [DataRow(MethodAttributes.Public | MethodAttributes.Final, "Public Final", DisplayName = "PublicFinal")]
        [DataRow(MethodAttributes.Public | MethodAttributes.Virtual, "Public Virtual", DisplayName = "PublicVirtual")]
        [DataRow(MethodAttributes.Public | MethodAttributes.HideBySig, "Public HideBySig", DisplayName = "PublicHideBySig")]
        [DataRow(MethodAttributes.Public | MethodAttributes.NewSlot, "Public", DisplayName = "Public no NewSlot")]
        [DataRow(MethodAttributes.Public | MethodAttributes.Abstract, "Public Abstract", DisplayName = "PublicAbstract")]
        [DataRow(MethodAttributes.Public | MethodAttributes.UnmanagedExport, "Public UnmanagedExport", DisplayName = "PublicUnmanagedExport")]
        [DataRow(MethodAttributes.Public | MethodAttributes.CheckAccessOnOverride, "Public CheckAccessOnOverride", DisplayName = "PublicCheckAccessOnOverride")]
        [DataRow(MethodAttributes.Public | MethodAttributes.SpecialName, "Public SpecialName", DisplayName = "PublicSpecialName")]
        [DataRow(MethodAttributes.Public | MethodAttributes.RTSpecialName, "Public RTSpecialName", DisplayName = "PublicRTSpecialName")]
        [DataRow(MethodAttributes.Public | MethodAttributes.PinvokeImpl, "Public PinvokeImpl", DisplayName = "PublicPinvokeImpl")]
        [DataRow(MethodAttributes.Public | MethodAttributes.HasSecurity, "Public HasSecurity", DisplayName = "PublicHasSecurity")]
        [DataRow(MethodAttributes.Public | MethodAttributes.RequireSecObject, "Public RequireSecObject", DisplayName = "PublicRequireSecObject")]
        [DataRow(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, "Public Virtual HideBySig", DisplayName = "PublicVirtualHideBySig")]
        [DataRow(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.Abstract, "Public Virtual HideBySig Abstract", DisplayName = "ComplexCombination")]
        public void DecodeToString_MethodAttributes_Combinations(MethodAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region PropertyAttributes Tests

        [DataTestMethod]
        [DataRow(PropertyAttributes.None, "", DisplayName = "None")]
        [DataRow(PropertyAttributes.HasDefault, "HasDefault", DisplayName = "HasDefault")]
        [DataRow(PropertyAttributes.SpecialName, "SpecialName", DisplayName = "SpecialName")]
        [DataRow(PropertyAttributes.RTSpecialName, "RTSpecialName", DisplayName = "RTSpecialName")]
        [DataRow(PropertyAttributes.HasDefault | PropertyAttributes.SpecialName, "HasDefault SpecialName", DisplayName = "HasDefaultAndSpecialName")]
        [DataRow(PropertyAttributes.HasDefault | PropertyAttributes.SpecialName | PropertyAttributes.RTSpecialName, "HasDefault RTSpecialName SpecialName", DisplayName = "AllFlags")]
        public void DecodeToString_PropertyAttributes(PropertyAttributes attrs, string expected)
        {
            var result = OrderedTypeList.DecodeToString(attrs);
            Assert.AreEqual(expected, result);
        }

        #endregion
    }
}
