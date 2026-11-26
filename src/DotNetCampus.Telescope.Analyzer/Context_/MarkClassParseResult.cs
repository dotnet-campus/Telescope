using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 被标记的类型的转换结果，约等于生成代码前的最终结果。
/// </summary>
internal readonly struct MarkClassParseResult(
    INamedTypeSymbol exportedTypeSymbol,
    AttributeData matchAssemblyMarkAttributeData,
    MarkExportAttributeParseResult markExportAttributeParseResult)
{
    /// <summary>
    /// 导出的 class 类型的语义。
    /// </summary>
    public INamedTypeSymbol ExportedTypeSymbol { get; } = exportedTypeSymbol;

    /// <summary>
    /// 类型上标记的程序集指定特性的语义。
    /// </summary>
    public AttributeData MatchAssemblyMarkAttributeData { get; } = matchAssemblyMarkAttributeData;

    /// <summary>
    /// 程序集特性里面的定义结果。
    /// </summary>
    public MarkExportAttributeParseResult MarkExportAttributeParseResult { get; } = markExportAttributeParseResult;
}
