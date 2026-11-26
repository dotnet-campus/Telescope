using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.Models;

/// <summary>
/// 表示 ValueTuple 的每一项的定义内容。
/// </summary>
internal class ValueTupleItemSyntaxAndSymbolInfo(ITypeSymbol itemType, string itemName)
{
    /// <summary>
    /// 类型。
    /// </summary>
    public ITypeSymbol ItemType { get; } = itemType;

    /// <summary>
    /// 命名。如果没有命名，那就是空字符串。
    /// </summary>
    public string ItemName { get; } = itemName;
}
