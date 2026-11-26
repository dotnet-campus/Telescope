using DotNetCampus.Telescope.Generators;
using DotNetCampus.Telescope.Tests.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetCampus.Telescope.Tests;

[TestClass]
public class ExportTypeToMethodTest
{
    /// <summary>
    /// 测试包含两个类型的 ValueTuple 返回值的情况：public class + private static method。
    /// </summary>
    [TestMethod]
    public void TestTwoTypeValueTupleWithPublicClassAndPrivateStaticMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                private static partial IEnumerable<(Type, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("public partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("private static partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("yield return (typeof(global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo), () => new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo());"));
        Assert.IsTrue(sourceText.Contains("yield return (typeof(global::DotNetCampus.Telescope.Tests.Lib2.F2), () => new global::DotNetCampus.Telescope.Tests.Lib2.F2());"));
        Assert.IsTrue(sourceText.Contains("yield return (typeof(global::DotNetCampus.Telescope.Tests.Lib3.F3), () => new global::DotNetCampus.Telescope.Tests.Lib3.F3());"));
        Assert.IsTrue(sourceText.Contains("yield return (typeof(global::DotNetCampus.Telescope.Tests.Lib3.FooWithoutAttribute), () => new global::DotNetCampus.Telescope.Tests.Lib3.FooWithoutAttribute());"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况（包含引用程序集）：public class + private static method。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleIncludeReferencesWithPublicClassAndPrivateStaticMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                private static partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("public partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("private static partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
        Assert.IsFalse(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.FooWithoutAttribute()"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况（不包含引用程序集）：public class + private method。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleWithPublicClassAndPrivateMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class TestCode
            {
                [TelescopeExport(IncludeReferences = false)]
                private partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("public partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("private partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsFalse(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsFalse(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
        Assert.IsFalse(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.FooWithoutAttribute()"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况：public class + private method + IncludeReferences = true。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleIncludeReferencesWithPublicClassAndPrivateMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                private partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("public partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("private partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
        Assert.IsFalse(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.FooWithoutAttribute()"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况：默认访问级别 class + public method。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleIncludeReferencesWithDefaultInternalClassAndPublicMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                public partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("public partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况：internal class + public static method。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleIncludeReferencesWithInternalClassAndPublicStaticMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            internal partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                public static partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("internal partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("public static partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
    }

    /// <summary>
    /// 测试包含三个类型的 ValueTuple 返回值的情况：public class + protected static method。
    /// </summary>
    [TestMethod]
    public void TestThreeTypeValueTupleIncludeReferencesWithPublicClassAndProtectedStaticMethod()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;
            using DotNetCampus.Telescope.Tests.Lib1;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class TestCode
            {
                [TelescopeExport(IncludeReferences = true)]
                protected static partial IEnumerable<(Type, F1Attribute xx, Func<DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("public partial class TestCode"));
        Assert.IsTrue(sourceText.Contains("protected static partial global::System.Collections.Generic.IEnumerable<(global::System.Type, global::DotNetCampus.Telescope.Tests.Lib1.F1Attribute xx, global::System.Func<global::DotNetCampus.Telescope.Tests.Lib1.F1> xxx)> ExportFooEnumerable()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Assets.CurrentFoo()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib2.F2()"));
        Assert.IsTrue(sourceText.Contains("new global::DotNetCampus.Telescope.Tests.Lib3.F3()"));
    }

    /// <summary>
    /// 测试特性参数包含数组、转义字符串、枚举、typeof 与 null 时的代码生成与编译。
    /// </summary>
    [TestMethod]
    public void TestComplexAttributeArgumentsCompileCleanly()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class ComplexTestCode
            {
                [TelescopeExport]
                public static partial IEnumerable<(Type type, FooAttribute attribute, Func<Base> creator)> ExportFoos();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();

        Assert.IsTrue(sourceText.Contains("global::DotNetCampus.Telescope.Tests.Assets.FooWithComplexAttribute"));
        Assert.IsTrue(sourceText.Contains("Tags = new string[] { \"a\", \"b\" }"));
    }

    /// <summary>
    /// 测试未匹配任何类型时生成 yield break 且可正常编译。
    /// </summary>
    [TestMethod]
    public void TestNoMatchingTypesGeneratesYieldBreak()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public interface IUnimplemented { }

            public partial class EmptyTestCode
            {
                [TelescopeExport]
                public static partial IEnumerable<(Type type, Func<IUnimplemented> creator)> ExportNone();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Length);
        AssertNoCompilationErrors(outputCompilation);

        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        Assert.AreEqual(1, generatedTrees.Length);
        var sourceText = generatedTrees[0].GetText().ToString();
        Assert.IsTrue(sourceText.Contains("yield break;"));
    }

    /// <summary>
    /// 测试返回值类型不符合约定时报告 Tes001 诊断。
    /// </summary>
    [TestMethod]
    public void TestInvalidReturnTypeReportsTes001()
    {
        var compilation = TestCodeProvider.CreateCompilation();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using DotNetCampus.Telescope;

            using System;
            using System.Collections.Generic;

            namespace DotNetCampus.Telescope.Tests.Assets;

            public partial class InvalidTestCode
            {
                [TelescopeExport]
                private static partial IEnumerable<(string wrongFirst, Func<Base> creator)> ExportInvalid();
            }
            """));

        var generator = new TelescopeExportTypeToMethodIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("Tes001", diagnostics[0].Id);
    }

    private static void AssertNoCompilationErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors.Select(static e => e.ToString())));
    }
}
