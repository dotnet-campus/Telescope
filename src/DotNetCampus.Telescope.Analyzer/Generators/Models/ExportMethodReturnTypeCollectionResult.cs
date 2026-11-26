using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.Models;

/// <summary>
/// 导出的方法的导出类型返回值结果。
/// </summary>
internal class ExportMethodReturnTypeCollectionResult(
    ITypeSymbol expectedClassBaseType,
    ITypeSymbol? expectedClassAttributeType,
    ExportTypeCollectionResult exportTypeCollectionResult,
    ExportMethodReturnType exportMethodReturnType)
    : IExportMethodReturnTypeCollectionResult, IEquatable<ExportMethodReturnTypeCollectionResult>
{
    /// <summary>
    /// 期望收集的类型所继承的基础类型。
    /// </summary>
    public ITypeSymbol ExpectedClassBaseType { get; } = expectedClassBaseType;

    /// <summary>
    /// 期望类型标记的特性，可选。
    /// </summary>
    public ITypeSymbol? ExpectedClassAttributeType { get; } = expectedClassAttributeType;

    public ExportTypeCollectionResult ExportTypeCollectionResult { get; } = exportTypeCollectionResult;

    /// <summary>
    /// 程序集里面标记了导出的分部方法，将用来生成代码。
    /// </summary>
    public IMethodSymbol ExportPartialMethodSymbol => ExportTypeCollectionResult.ExportPartialMethodSymbol;

    /// <summary>
    /// 导出类型的返回类型信息。
    /// </summary>
    public ExportMethodReturnType ExportMethodReturnType { get; } = exportMethodReturnType;

    /// <summary>
    /// 判断传入的程序集类型满足当前的要求条件。
    /// </summary>
    public bool IsMatch(INamedTypeSymbol assemblyClassTypeSymbol)
    {
        if (assemblyClassTypeSymbol.TypeKind != TypeKind.Class
            || assemblyClassTypeSymbol.IsAbstract
            || assemblyClassTypeSymbol.IsStatic)
        {
            return false;
        }

        if (!assemblyClassTypeSymbol.IsInheritFrom(ExpectedClassBaseType))
        {
            return false;
        }

        if (ExpectedClassAttributeType is null)
        {
            return true;
        }

        foreach (var attributeData in assemblyClassTypeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, ExpectedClassAttributeType))
            {
                return true;
            }
        }

        return false;
    }

    public bool Equals(ExportMethodReturnTypeCollectionResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ExportTypeCollectionResult.Equals(other.ExportTypeCollectionResult)
            && ExportMethodReturnType == other.ExportMethodReturnType
            && SymbolEqualityComparer.Default.Equals(ExpectedClassBaseType, other.ExpectedClassBaseType)
            && SymbolEqualityComparer.Default.Equals(ExpectedClassAttributeType, other.ExpectedClassAttributeType);
    }

    public override bool Equals(object? obj) => obj is ExportMethodReturnTypeCollectionResult other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ExportTypeCollectionResult.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)ExportMethodReturnType;
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(ExpectedClassBaseType);
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(ExpectedClassAttributeType);
            return hashCode;
        }
    }
}
