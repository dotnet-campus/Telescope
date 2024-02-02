using System.IO;

namespace dotnetCampus.Telescope.SourceGenerator.Test.Utils;

internal static class TestCodeProvider
{
    public static string GetTestCode()
    {
        var manifestResourceStream = typeof(TestCodeProvider).Assembly.GetManifestResourceStream("dotnetCampus.Telescope.SourceGenerator.Test.Assets.TestCode.cs")!;
        var streamReader = new StreamReader(manifestResourceStream);
        return streamReader.ReadToEnd();
    }
}