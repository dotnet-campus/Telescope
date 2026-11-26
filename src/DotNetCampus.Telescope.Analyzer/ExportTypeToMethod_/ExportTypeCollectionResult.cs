using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

internal class ExportTypeCollectionResult(IMethodSymbol methodSymbol, SyntaxNode targetSyntaxNode) : IEquatable<ExportTypeCollectionResult>
{
    /// <summary>
    /// 是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集。
    /// </summary>
    public bool IncludeReference { get; set; }

    /// <summary>
    /// 程序集里面标记了导出的分部方法，将用来生成代码。
    /// </summary>
    public IMethodSymbol ExportPartialMethodSymbol { get; } = methodSymbol;

    public SyntaxNode TargetSyntaxNode { get; } = targetSyntaxNode;

    public bool Equals(ExportTypeCollectionResult? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IncludeReference == other.IncludeReference
            && TargetSyntaxNode.Equals(other.TargetSyntaxNode)
            && SymbolEqualityComparer.Default.Equals(ExportPartialMethodSymbol, other.ExportPartialMethodSymbol);
    }

    public override bool Equals(object? obj) => obj is ExportTypeCollectionResult other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SymbolEqualityComparer.Default.GetHashCode(ExportPartialMethodSymbol);
            hashCode = (hashCode * 397) ^ TargetSyntaxNode.GetHashCode();
            hashCode = (hashCode * 397) ^ IncludeReference.GetHashCode();
            return hashCode;
        }
    }
}
