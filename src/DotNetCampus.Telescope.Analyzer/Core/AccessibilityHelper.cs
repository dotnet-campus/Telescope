using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal static class AccessibilityHelper
{
    public static string ToCSharpCode(this Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => string.Empty,
        };
}
