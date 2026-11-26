namespace DotNetCampus.Telescope.Generators.Models;

/// <summary>
/// 导出方法的返回类型。
/// </summary>
public enum ExportMethodReturnType
{
    /// <summary>
    /// 采用 IEnumerable 导出 ValueTuple，包含 Type、可选 Attribute 和 Creator 参数。
    /// </summary>
    EnumerableValueTupleWithTypeAttributeCreator,
}
