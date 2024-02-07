using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using dotnetCampus.Telescope.SourceGenerator.Test.Utils;
using dotnetCampus.Telescope.SourceGeneratorAnalyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetCampus.Telescope.SourceGenerator.Test;

[TestClass]
public class TelescopeIncrementalGeneratorTest
{
    [TestMethod]
    public void TestCollectionAssembly()
    {
        var compilation = TestCodeProvider.CreateCompilation();

        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(@"
using dotnetCampus.Telescope;
using dotnetCampus.Telescope.SourceGenerator.Test.Assets;

[assembly: MarkExport(typeof(Base), typeof(FooAttribute))]"));

        var generator = new TelescopeIncrementalGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);

        var attributedTypesExportCode = outputCompilation.SyntaxTrees.FirstOrDefault(t => Path.GetFileName(t.FilePath) == "__AttributedTypesExport__.cs");
        Assert.IsNotNull(attributedTypesExportCode);
        var codeText = attributedTypesExportCode.GetText().ToString();
        // 内容如下
        /*
            public partial class __AttributedTypesExport__ : ICompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base,  lobal::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute>
            {
                AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base, global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute>[]  CompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base,  lobal::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute>.ExportAttributeTypes()
                {
                    return new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base, lobal::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute> []
                    {
                        new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base,  global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute>typeof(global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Foo), new  global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute() { }, () => ew global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Foo())
                    };
                }
            }
         */
        Assert.AreEqual(true, codeText.Contains("global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Base"));
        Assert.AreEqual(true, codeText.Contains("global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.FooAttribute"));
        Assert.AreEqual(true, codeText.Contains("typeof(global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Foo)"));
        Assert.AreEqual(true, codeText.Contains("new global::dotnetCampus.Telescope.SourceGenerator.Test.Assets.Foo()"));
    }
}
