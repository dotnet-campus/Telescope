using DotNetCampus.Telescope.Generators.Builders;
using DotNetCampus.Telescope.Generators.Models;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.CodeTransforming;

/// <summary>
/// 导出方法的生成器。
/// </summary>
internal interface IExportMethodCodeGenerator
{
    void BuildMethod(
        TypeDeclarationSourceTextBuilder typeBuilder,
        ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
        List<INamedTypeSymbol> list,
        CancellationToken token);
}
