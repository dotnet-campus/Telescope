using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.Models;

internal readonly struct MarkExportAttributeParseResult(
    bool success,
    ITypeSymbol baseClassOrInterfaceTypeInfo,
    ITypeSymbol attributeTypeInfo) : IEquatable<MarkExportAttributeParseResult>
{
    public bool Success { get; } = success;

    public ITypeSymbol BaseClassOrInterfaceTypeInfo { get; } = baseClassOrInterfaceTypeInfo;

    public ITypeSymbol AttributeTypeInfo { get; } = attributeTypeInfo;

    /// <summary>
    /// 获取表示失败的特性解析结果。
    /// </summary>
    public static MarkExportAttributeParseResult Failure { get; } = new(false, default!, default!);

    public bool Equals(MarkExportAttributeParseResult other)
    {
        return Success == other.Success
            && SymbolEqualityComparer.Default.Equals(BaseClassOrInterfaceTypeInfo, other.BaseClassOrInterfaceTypeInfo)
            && SymbolEqualityComparer.Default.Equals(AttributeTypeInfo, other.AttributeTypeInfo);
    }

    public override bool Equals(object? obj) => obj is MarkExportAttributeParseResult other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Success.GetHashCode();
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(BaseClassOrInterfaceTypeInfo);
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(AttributeTypeInfo);
            return hashCode;
        }
    }
}
