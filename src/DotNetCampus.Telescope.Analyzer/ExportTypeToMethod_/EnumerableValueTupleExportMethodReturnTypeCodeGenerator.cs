using System.Text;

using DotNetCampus.Telescope.Analyzer.Core;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 采用 <see cref="IEnumerable{T}"/> 的 <see cref="ValueTuple"/> 返回值类型的生成方法。
/// </summary>
internal class EnumerableValueTupleExportMethodReturnTypeCodeGenerator : IExportMethodCodeGenerator
{
    public string GenerateSourceCode(
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        if (exportMethodReturnTypeCollectionResult.ExportMethodReturnType !=
            ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator)
        {
            throw new ArgumentException("调用错误，其他返回值类型不应该调用。");
        }

        var methodCode = new StringBuilder();

        if (list.Count == 0)
        {
            methodCode.Append("yield break;");
        }
        else
        {
            foreach (var namedTypeSymbol in list)
            {
                token.ThrowIfCancellationRequested();

                var typeName = TypeSymbolHelper.TypeSymbolToFullName(namedTypeSymbol);
                if (exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType is null)
                {
                    methodCode.AppendLine($"yield return (typeof({typeName}), () => new {typeName}());");
                }
                else
                {
                    var attribute = namedTypeSymbol.GetAttributes().First(t =>
                        SymbolEqualityComparer.Default.Equals(
                            t.AttributeClass,
                            exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType));
                    var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(attribute);

                    methodCode.AppendLine($"yield return (typeof({typeName}), {attributeCreatedCode}, () => new {typeName}());");
                }
            }
        }

        return SourceCodeGeneratorHelper.GeneratePartialMethodCode(
            exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol,
            methodCode.ToString().TrimEnd(),
            token);
    }
}
