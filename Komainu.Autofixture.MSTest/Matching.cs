using System;

namespace Komainu.AutoFixture.MSTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Justification = "This enumeration is designed to be used together with an attribute and is named to improve readability.")]
    [Flags]
    public enum Matching
    {
        ExactType = 1,
        DirectBaseType = 2,
        ImplementedInterfaces = 4,
        ParameterName = 8,
        PropertyName = 16,
        FieldName = 32,
        MemberName = ParameterName | PropertyName | FieldName
    }
}