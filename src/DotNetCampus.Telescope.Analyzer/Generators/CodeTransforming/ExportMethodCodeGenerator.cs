using DotNetCampus.Telescope.Generators.Builders;
using DotNetCampus.Telescope.Generators.Models;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.CodeTransforming;

/// <summary>
/// 生成导出类型方法的代码分发工厂。
/// </summary>
internal static class ExportMethodCodeGenerator
{
    public static void BuildMethod(
        TypeDeclarationSourceTextBuilder typeBuilder,
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        IExportMethodCodeGenerator codeGenerator = exportMethodReturnTypeCollectionResult.ExportMethodReturnType switch
        {
            ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator =>
                new EnumerableValueTupleExportMethodCodeGenerator(),
            _ => throw new NotSupportedException(),
        };

        codeGenerator.BuildMethod(typeBuilder, exportMethodReturnTypeCollectionResult, list, token);
    }
}
