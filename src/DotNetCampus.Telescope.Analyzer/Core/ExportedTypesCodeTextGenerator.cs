using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal class ExportedTypesCodeTextGenerator
{
    public string Generate(ImmutableArray<MarkClassParseResult> markClassCollection, CancellationToken token)
    {
        if (markClassCollection.Length == 0)
        {
            // 如果没有任何需要导出的类型，那就不要创建任何代码
            return string.Empty;
        }

        // 导出的接口
        var exportedInterfaces = new List<string>();
        // 导出的方法
        var exportedMethodCodes = new List<string>();

        foreach (var markClassGroup in markClassCollection.GroupBy(static t => t.MarkExportAttributeParseResult))
        {
            token.ThrowIfCancellationRequested();

            var markExportAttributeParseResult = markClassGroup.Key;
            var baseClassOrInterfaceName = TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo);
            var attributeName = TypeSymbolHelper.TypeSymbolToFullName(markExportAttributeParseResult.AttributeTypeInfo);

            var exportedItemList = new List<string>();
            var seenTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var markClassParseResult in markClassGroup)
            {
                token.ThrowIfCancellationRequested();

                if (!seenTypes.Add(markClassParseResult.ExportedTypeSymbol))
                {
                    continue;
                }

                var typeName = TypeSymbolHelper.TypeSymbolToFullName(markClassParseResult.ExportedTypeSymbol);
                var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(markClassParseResult.MatchAssemblyMarkAttributeData);

                var itemCode =
                    $"new global::DotNetCampus.Telescope.AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), {attributeCreatedCode}, () => new {typeName}())";
                exportedItemList.Add(itemCode);
            }

            var arrayExpression = $$"""
                new global::DotNetCampus.Telescope.AttributedTypeMetadata<{{baseClassOrInterfaceName}}, {{attributeName}}>[]
                {
                    {{string.Join(",\n    ", exportedItemList)}}
                }
                """;

            var methodCode = $$"""
                global::DotNetCampus.Telescope.AttributedTypeMetadata<{{baseClassOrInterfaceName}}, {{attributeName}}>[] global::DotNetCampus.Telescope.ICompileTimeAttributedTypesExporter<{{baseClassOrInterfaceName}}, {{attributeName}}>.ExportAttributeTypes()
                {
                    return {{arrayExpression}};
                }
                """;

            exportedMethodCodes.Add(methodCode);
            exportedInterfaces.Add($"global::DotNetCampus.Telescope.ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>");
        }

        var code = $$"""
            {{AssemblyInfo.GeneratedCodeComment}}namespace {{InternalContracts.GeneratedCodeNamespace}}
            {
                public partial class {{InternalContracts.AttributedTypesExportClassName}} : {{string.Join(", ", exportedInterfaces)}}
                {
            {{SourceCodeGeneratorHelper.IndentSource(string.Join("\n\n", exportedMethodCodes), 2, shouldFirstLine: true)}}
                }
            }
            """;

        return code;
    }
}
