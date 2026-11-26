using DotNetCampus.Telescope.Generators.Builders;
using DotNetCampus.Telescope.Generators.Models;
using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.CodeTransforming;

/// <summary>
/// 采用 <see cref="IEnumerable{T}"/> 的 <see cref="ValueTuple"/> 返回值类型的生成方法。
/// </summary>
internal sealed class EnumerableValueTupleExportMethodCodeGenerator : IExportMethodCodeGenerator
{
    public void BuildMethod(
        TypeDeclarationSourceTextBuilder typeBuilder,
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        if (exportMethodReturnTypeCollectionResult.ExportMethodReturnType !=
            ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator)
        {
            throw new ArgumentException("调用错误，其他返回值类型不应该调用。");
        }

        var signature = PartialTypeSourceCodeGenerator.BuildPartialMethodSignature(
            exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol,
            token);

        typeBuilder.AddMethodDeclaration(signature, method =>
        {
            if (list.Count == 0)
            {
                method.AddRawStatement("yield break;");
                return;
            }

            foreach (var namedTypeSymbol in list)
            {
                token.ThrowIfCancellationRequested();

                var typeName = namedTypeSymbol.ToGlobalNameWithoutGenerics();
                if (exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType is null)
                {
                    method.AddRawStatement($"yield return (typeof({typeName}), () => new {typeName}());");
                }
                else
                {
                    var attribute = namedTypeSymbol.GetAttributes().First(t =>
                        SymbolEqualityComparer.Default.Equals(
                            t.AttributeClass,
                            exportMethodReturnTypeCollectionResult.ExpectedClassAttributeType));
                    var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(attribute);

                    method.AddRawStatement($"yield return (typeof({typeName}), {attributeCreatedCode}, () => new {typeName}());");
                }
            }
        });
    }
}
