using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

using System.IO;
using System.Linq;

namespace dotnetCampus.Telescope.SourceGenerator.Test.Utils;

internal static class TestCodeProvider
{
    public static string GetTestCode()
    {
        var manifestResourceStream = typeof(TestCodeProvider).Assembly.GetManifestResourceStream("dotnetCampus.Telescope.SourceGenerator.Test.Assets.TestCode.cs")!;
        var streamReader = new StreamReader(manifestResourceStream);
        return streamReader.ReadToEnd();
    }

    public static CSharpCompilation CreateCompilation()
    {
        return CreateCompilation(GetTestCode());
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(MarkExportAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(dotnetCampus.Telescope.SourceGeneratorAnalyzers.TestLib1.F1)
                    .Assembly.Location),
                MetadataReference.CreateFromFile(typeof(dotnetCampus.Telescope.SourceGeneratorAnalyzers.TestLib2.F2)
                    .Assembly.Location),
                MetadataReference.CreateFromFile(typeof(dotnetCampus.Telescope.SourceGeneratorAnalyzers.TestLib3.F3)
                    .Assembly.Location),
            }.Concat(MetadataReferenceProvider.GetDotNetMetadataReferenceList()),
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}