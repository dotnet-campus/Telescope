using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 导出方法的生成器。
/// </summary>
internal interface IExportMethodCodeGenerator
{
    string GenerateSourceCode(
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token);
}
