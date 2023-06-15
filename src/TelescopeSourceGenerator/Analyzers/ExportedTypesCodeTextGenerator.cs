﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

class ExportedTypesCodeTextGenerator
{
    public string Generate(ImmutableArray<MarkClassParseResult> markClassCollection, CancellationToken token)
    {
        // 导出的接口
        var exportedInterfaces = new List<string>();
        // 导出的方法
        var exportedMethodCodes = new List<string>();

        foreach (var markClassGroup in markClassCollection.GroupBy(t => t.MarkExportAttributeParseResult))
        {
            token.ThrowIfCancellationRequested();

            var markExportAttributeParseResult = markClassGroup.Key;

            var baseClassOrInterfaceName =
                TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo);
            var attributeName = TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.AttributeTypeInfo);

            var exportedItemList = new List<string>();

            foreach (var markClassParseResult in markClassGroup)
            {
                var typeName = TypeSymbolHelper.TypeSymbolToFullName(markClassParseResult.ExportedTypeSymbol);

                var attributeCreatedCode =
                    AttributeCodeReWriter.GetAttributeCreatedCode(markClassParseResult.MatchAssemblyMarkAttributeData);

                var itemCode =
                    @$"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), {attributeCreatedCode}, () => new {typeName}())";
                exportedItemList.Add(itemCode);
            }

            var arrayExpression = $@"new AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[]
            {{
                {string.Join(@",
                ", exportedItemList)}
            }}";

            var methodCode =
                $@"AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>[] ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>.ExportAttributeTypes()
        {{
            return {arrayExpression};
        }}";

            exportedMethodCodes.Add(methodCode);

            exportedInterfaces.Add(
                $@"ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>");
        }

        var code = $@"using dotnetCampus.Telescope;

namespace dotnetCampus.Telescope
{{
    public partial class __AttributedTypesExport__ : {string.Join(", ", exportedInterfaces)}
    {{
        {string.Join(@"
        ", exportedMethodCodes)}
    }}
}}";
        code = FormatCode(code);
        // 生成的代码示例：
        /*
using dotnetCampus.Telescope;

namespace dotnetCampus.Telescope
{
    public partial class __AttributedTypesExport__ : ICompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>
    {
        AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[] ICompileTimeAttributedTypesExporter<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>.ExportAttributeTypes()
        {
            return new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>[]
            {
                new AttributedTypeMetadata<global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base, global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute>
                (
                   typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo), 
                   new global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooAttribute(1, (global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooEnum)1, typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Base), null) 
                   {
                       Number2 = 2, 
                       Type2 = typeof(global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo),
                       FooEnum2 = (global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.FooEnum)0,
                       Type3 = null 
                   }, 
                   () => new global::dotnetCampus.Telescope.SourceGeneratorAnalyzers.Demo.Foo()
                )
            };
        }
    }
}
         */
        return code;
    }

    /// <summary>
    /// 格式化代码。
    /// </summary>
    /// <param name="sourceCode">未格式化的源代码。</param>
    /// <returns>格式化的源代码。</returns>
    private static string FormatCode(string sourceCode)
    {
        var rootSyntaxNode = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();
        return rootSyntaxNode.NormalizeWhitespace().ToFullString();
    }
}

static class TypeSymbolHelper
{
    /// <summary>
    /// 输出类型的完全限定名
    /// </summary>
    public static string TypeSymbolToFullName(ITypeSymbol typeSymbol)
    {
        // 带上 global 格式的输出 FullName 内容
        var symbolDisplayFormat = new SymbolDisplayFormat
        (
            // 带上命名空间和类型名
            SymbolDisplayGlobalNamespaceStyle.Included,
            // 命名空间之前加上 global 防止冲突
            SymbolDisplayTypeQualificationStyle
                .NameAndContainingTypesAndNamespaces
        );

        return typeSymbol.ToDisplayString(symbolDisplayFormat);
    }
}

static class AttributeCodeReWriter
{
    /// <summary>
    /// 从 <paramref name="attributeData"/> 转换为特性生成代码。从 `[Foo(xx, xxx)]` 语义转换为 `new Foo(xx, xxx)` 的生成代码
    /// </summary>
    /// <param name="attributeData"></param>
    /// <returns></returns>
    public static string GetAttributeCreatedCode(AttributeData attributeData)
    {
        // 放在特性的构造函数的参数列表，例如 [Foo(1,2,3)] 将会获取到 `1` `2` `3` 三个参数
        var constructorArgumentCodeList = new List<string>();
        foreach (TypedConstant constructorArgument in attributeData.ConstructorArguments)
        {
            var constructorArgumentCode = TypedConstantToCodeString(constructorArgument);

            constructorArgumentCodeList.Add(constructorArgumentCode);
        }

        var namedArgumentCodeList = new List<(string propertyName, string valueCode)>();
        foreach (var keyValuePair in attributeData.NamedArguments)
        {
            var key = keyValuePair.Key;

            var typedConstant = keyValuePair.Value;
            var argumentCode = TypedConstantToCodeString(typedConstant);

            namedArgumentCodeList.Add((key, argumentCode));
        }

        return
            $@"new {TypeSymbolHelper.TypeSymbolToFullName(attributeData.AttributeClass!)}({string.Join(",", constructorArgumentCodeList)})
{{
           {string.Join(@",
                        ", namedArgumentCodeList.Select(x => $"{x.propertyName} = {x.valueCode}"))}
}}";

        static string TypedConstantToCodeString(TypedConstant typedConstant)
        {
            var constructorArgumentType = typedConstant.Type;
            var constructorArgumentValue = typedConstant.Value;

            string constructorArgumentCode;
            switch (typedConstant.Kind)
            {
                case TypedConstantKind.Enum:
                {
                    // "(Foo.Enum1) 1"
                    constructorArgumentCode =
                        $"({TypeSymbolHelper.TypeSymbolToFullName(typedConstant.Type!)}) {typedConstant.Value}";
                    break;
                }
                case TypedConstantKind.Type:
                {
                    var typeSymbol = (ITypeSymbol?)constructorArgumentValue;
                    if (typeSymbol is null)
                    {
                        constructorArgumentCode = "null";
                    }
                    else
                    {
                        constructorArgumentCode = $"typeof({TypeSymbolHelper.TypeSymbolToFullName(typeSymbol)})";
                    }

                    break;
                }
                default:
                {
                    constructorArgumentCode = typedConstant.Value?.ToString() ?? "null";
                    break;
                }
            }

            return constructorArgumentCode;
        }
    }
}