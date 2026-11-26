using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 候选的导出类型结果。
/// 包含标记导出的方法信息，以及程序集里面导出的类型。
/// </summary>
internal class CandidateClassTypeResult(
    ExportMethodReturnTypeCollectionResult exportMethodReturnTypeCollectionResult,
    IReadOnlyList<INamedTypeSymbol> assemblyClassTypeSymbolList)
{
    public ExportMethodReturnTypeCollectionResult ExportMethodReturnTypeCollectionResult { get; } = exportMethodReturnTypeCollectionResult;

    public IReadOnlyList<INamedTypeSymbol> AssemblyClassTypeSymbolList { get; } = assemblyClassTypeSymbolList;
}
