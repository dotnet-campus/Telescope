using Microsoft.CodeAnalysis;

using static DotNetCampus.Telescope.Properties.Resources;
using static Microsoft.CodeAnalysis.WellKnownDiagnosticTags;

namespace DotNetCampus.Telescope.Analyzer.Diagnostics;

internal static class TesDiagnostics
{
    // ReSharper disable InconsistentNaming
    public static DiagnosticDescriptor Tes000_UnknownError { get; } = new(
        nameof(Tes000),
        Localize(nameof(Tes000)),
        Localize(nameof(Tes000_Message)),
        Categories.Compiler,
        DiagnosticSeverity.Error,
        true,
        customTags: [AnalyzerException, NotConfigurable]);

    public static DiagnosticDescriptor Tes001_MethodReturnTypeError { get; } = new(
        nameof(Tes001),
        Localize(nameof(Tes001)),
        Localize(nameof(Tes001_Message)),
        Categories.Compiler,
        DiagnosticSeverity.Error,
        true,
        customTags: [AnalyzerException, NotConfigurable]);

    private static class Categories
    {
        /// <summary>
        /// 可能产生 bug，则报告此诊断。
        /// </summary>
        public const string AvoidBugs = "DotNetCampus.AvoidBugs";

        /// <summary>
        /// 为了提供代码生成能力，则报告此诊断。
        /// </summary>
        public const string CodeFixOnly = "DotNetCampus.CodeFixOnly";

        /// <summary>
        /// 因编译要求而必须满足的条件没有满足，则报告此诊断。
        /// </summary>
        public const string Compiler = "DotNetCampus.Compiler";

        /// <summary>
        /// 因 Telescope 库内的机制限制，必须满足此要求 Telescope 才可正常工作，则报告此诊断。
        /// </summary>
        public const string Mechanism = "DotNetCampus.Mechanism";

        /// <summary>
        /// 为了代码可读性，使之更易于理解、方便调试，则报告此诊断。
        /// </summary>
        public const string Readable = "DotNetCampus.Readable";

        /// <summary>
        /// 能写得出来正常编译，但会引发运行时异常，则报告此诊断。
        /// </summary>
        public const string RuntimeException = "DotNetCampus.RuntimeException";

        /// <summary>
        /// 编写了无法生效的代码，则报告此诊断。
        /// </summary>
        public const string Useless = "DotNetCampus.Useless";
    }

    public static LocalizableString Localize(string key) =>
        new LocalizableResourceString(key, ResourceManager, typeof(Properties.Resources));
}
