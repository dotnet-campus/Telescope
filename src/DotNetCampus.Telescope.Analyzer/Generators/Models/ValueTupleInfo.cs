namespace DotNetCampus.Telescope.Generators.Models;

/// <summary>
/// 表示 ValueTuple 的信息。
/// </summary>
internal class ValueTupleInfo(IReadOnlyList<ValueTupleItemSyntaxAndSymbolInfo> itemList)
{
    public IReadOnlyList<ValueTupleItemSyntaxAndSymbolInfo> ItemList { get; } = itemList;
}
