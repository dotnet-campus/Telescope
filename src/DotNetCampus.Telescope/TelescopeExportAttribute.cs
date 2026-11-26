using System;

namespace DotNetCampus.Telescope;

/// <summary>
/// 标记一个分部方法，指示 Telescope 源生成器为该方法生成类型导出代码。
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class TelescopeExportAttribute : Attribute
{
    /// <summary>
    /// 获取或设置是否包含引用的程序集和 DLL 里面的类型导出。默认只导出当前程序集。
    /// </summary>
    public bool IncludeReferences { get; set; }
}
