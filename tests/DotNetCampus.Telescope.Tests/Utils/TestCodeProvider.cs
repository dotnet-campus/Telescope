using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetCampus.Telescope.Tests.Utils;

internal static class TestCodeProvider
{
    public static string GetTestCode()
    {
        var manifestResourceStream = typeof(TestCodeProvider).Assembly
            .GetManifestResourceStream("DotNetCampus.Telescope.Tests.Assets.TestCode.cs")!;
        using var streamReader = new StreamReader(manifestResourceStream);
        return streamReader.ReadToEnd();
    }

    public static CSharpCompilation CreateCompilation(OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        return CreateCompilation(GetTestCode(), outputKind);
    }

    private static CSharpCompilation CreateCompilation(string source, OutputKind outputKind)
    {
        return CSharpCompilation.Create(
            "TestCompilation",
            [CSharpSyntaxTree.ParseText(source)],
            new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(MarkExportAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Lib1.F1).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Lib2.F2).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Lib3.F3).Assembly.Location),
            }.Concat(MetadataReferenceProvider.GetDotNetMetadataReferenceList()),
            new CSharpCompilationOptions(outputKind, nullableContextOptions: NullableContextOptions.Enable));
    }
}
