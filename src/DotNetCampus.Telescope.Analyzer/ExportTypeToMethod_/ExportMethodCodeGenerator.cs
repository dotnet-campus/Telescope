using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 生成导出类型方法的代码。
/// </summary>
internal static class ExportMethodCodeGenerator
{
    public static string GenerateSourceCode(
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token)
    {
        IExportMethodCodeGenerator codeGenerator = exportMethodReturnTypeCollectionResult.ExportMethodReturnType switch
        {
            ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator =>
                new EnumerableValueTupleExportMethodReturnTypeCodeGenerator(),
            _ => throw new NotSupportedException(),
        };

        return codeGenerator.GenerateSourceCode(exportMethodReturnTypeCollectionResult, list, token);
    }
}
