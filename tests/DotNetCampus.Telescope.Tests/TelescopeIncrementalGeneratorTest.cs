using System.Reflection;

using DotNetCampus.Telescope.Generators;
using DotNetCampus.Telescope.Tests.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetCampus.Telescope.Tests;

[TestClass]
public class TelescopeIncrementalGeneratorTest
{
    [TestMethod]
    public void TestCollectionAssembly()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Assets;

            [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
            """));

        var generator = new TelescopeIncrementalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);

        var errors = outputCompilation.GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors.Select(static e => e.ToString())));

        var attributedTypesExportCode = outputCompilation.SyntaxTrees
            .FirstOrDefault(static t => Path.GetFileName(t.FilePath) == "__AttributedTypesExport__.g.cs");
        Assert.IsNotNull(attributedTypesExportCode);

        var codeText = attributedTypesExportCode.GetText().ToString();
        Assert.IsTrue(codeText.Contains("global::DotNetCampus.Telescope.Tests.Assets.Base"));
        Assert.IsTrue(codeText.Contains("global::DotNetCampus.Telescope.Tests.Assets.FooAttribute"));
        Assert.IsTrue(codeText.Contains("typeof(global::DotNetCampus.Telescope.Tests.Assets.Foo)"));
        Assert.IsTrue(codeText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.Foo()"));
        Assert.IsTrue(codeText.Contains("typeof(global::DotNetCampus.Telescope.Tests.Assets.FooWithComplexAttribute)"));
    }

    [TestMethod]
    public void TestAttributedTypesFromAssemblyAtRuntime()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Assets;

            [assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
            """));

        var generator = new TelescopeIncrementalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        Assert.AreEqual(0, diagnostics.Length);

        using var peStream = new MemoryStream();
        var emitResult = outputCompilation.Emit(peStream);
        Assert.IsTrue(
            emitResult.Success,
            string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static d => d.ToString())));

        peStream.Position = 0;
        var loadedAssembly = Assembly.Load(peStream.ToArray());

        var baseType = loadedAssembly.GetType("DotNetCampus.Telescope.Tests.Assets.Base")!;
        var attrType = loadedAssembly.GetType("DotNetCampus.Telescope.Tests.Assets.FooAttribute")!;
        var fromAssemblyMethod = typeof(AttributedTypes)
            .GetMethod(nameof(AttributedTypes.FromAssembly), [typeof(Assembly[])])!
            .MakeGenericMethod(baseType, attrType);

        var results = ((System.Collections.IEnumerable)fromAssemblyMethod.Invoke(null, [new[] { loadedAssembly }])!)
            .Cast<object>()
            .ToList();

        Assert.AreEqual(2, results.Count);
    }
}
