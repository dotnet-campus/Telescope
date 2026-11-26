using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Utils.CodeAnalysis;

internal static class TypeSymbolExtensions
{
    private static readonly SymbolDisplayFormat FullNameWithoutGenericArgsDisplayFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    /// <summary>
    /// 输出不包含泛型实参的类型完全限定名（带 <c>global::</c> 前缀）。
    /// </summary>
    public static string ToGlobalNameWithoutGenerics(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(FullNameWithoutGenericArgsDisplayFormat);
    }

    /// <summary>
    /// 判断类型继承关系。
    /// </summary>
    /// <param name="currentType">当前的类型。</param>
    /// <param name="requiredType">需要继承的类型。</param>
    public static bool IsInheritFrom(this ITypeSymbol currentType, ITypeSymbol requiredType)
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
