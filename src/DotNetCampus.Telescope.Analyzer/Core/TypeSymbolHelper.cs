using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal static class TypeSymbolHelper
{
    private static readonly SymbolDisplayFormat FullNameDisplayFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    /// <summary>
    /// 输出类型的完全限定名。
    /// </summary>
    public static string TypeSymbolToFullName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(FullNameDisplayFormat);
    }

    /// <summary>
    /// 判断类型继承关系。
    /// </summary>
    /// <param name="currentType">当前的类型。</param>
    /// <param name="requiredType">需要继承的类型。</param>
    public static bool IsInherit(ITypeSymbol currentType, ITypeSymbol requiredType)
    {
        var baseType = currentType.BaseType;
        while (baseType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, requiredType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        foreach (var currentInheritInterfaceType in currentType.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(currentInheritInterfaceType, requiredType))
            {
                return true;
            }
        }

        return false;
    }
}
