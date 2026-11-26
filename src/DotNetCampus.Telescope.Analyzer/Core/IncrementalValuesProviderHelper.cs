using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal static class IncrementalValuesProviderHelper
{
    /// <summary>
    /// 过滤掉空的值。
    /// </summary>
    public static IncrementalValuesProvider<T> ExcludeNulls<T>(this IncrementalValuesProvider<T?> provider)
    {
        return provider.Where(static t => t is not null).Select(static (t, _) => t!);
    }
}
