using System.Collections.Immutable;

using DotNetCampus.Telescope.Generators.Builders;
using DotNetCampus.Telescope.Generators.Models;
using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.CodeTransforming;

internal sealed class ExportedTypesCodeGenerator
{
    public string Generate(ImmutableArray<MarkClassParseResult> markClassCollection, CancellationToken token)
    {
        if (markClassCollection.Length == 0)
        {
            return string.Empty;
        }

        var groups = markClassCollection
            .GroupBy(static t => t.MarkExportAttributeParseResult)
            .ToList();

        var exportedInterfaces = new List<string>(groups.Count);
        var groupDetails = new List<(string BaseTypeName, string AttributeTypeName, List<string> Items)>(groups.Count);

        foreach (var markClassGroup in groups)
        {
            token.ThrowIfCancellationRequested();

            var markExportAttributeParseResult = markClassGroup.Key;
            var baseClassOrInterfaceName = markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo.ToGlobalNameWithoutGenerics();
            var attributeName = markExportAttributeParseResult.AttributeTypeInfo.ToGlobalNameWithoutGenerics();

            var exportedItemList = new List<string>();
            var seenTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var markClassParseResult in markClassGroup)
            {
                token.ThrowIfCancellationRequested();

                if (!seenTypes.Add(markClassParseResult.ExportedTypeSymbol))
                {
                    continue;
                }

                var typeName = markClassParseResult.ExportedTypeSymbol.ToGlobalNameWithoutGenerics();
                var attributeCreatedCode = AttributeCodeReWriter.GetAttributeCreatedCode(markClassParseResult.MatchAssemblyMarkAttributeData);

                exportedItemList.Add(
                    $"new global::DotNetCampus.Telescope.AttributedTypeMetadata<{baseClassOrInterfaceName}, {attributeName}>(typeof({typeName}), {attributeCreatedCode}, () => new {typeName}()),");
            }

            exportedInterfaces.Add($"global::DotNetCampus.Telescope.ICompileTimeAttributedTypesExporter<{baseClassOrInterfaceName}, {attributeName}>");
            groupDetails.Add((baseClassOrInterfaceName, attributeName, exportedItemList));
        }

        using var builder = new SourceTextBuilder(InternalContracts.GeneratedCodeNamespace)
        {
            UseFileScopedNamespace = false,
            Nullable = null,
        };

        builder.AddTypeDeclaration($"public partial class {InternalContracts.AttributedTypesExportClassName}", type =>
        {
            type.AddGeneratedToolAndEditorBrowsingAttributes();
            type.AddBaseTypes(exportedInterfaces.ToArray());

            foreach (var (baseTypeName, attributeTypeName, items) in groupDetails)
            {
                var signature =
                    $"global::DotNetCampus.Telescope.AttributedTypeMetadata<{baseTypeName}, {attributeTypeName}>[] global::DotNetCampus.Telescope.ICompileTimeAttributedTypesExporter<{baseTypeName}, {attributeTypeName}>.ExportAttributeTypes()";
                type.AddMethodDeclaration(signature, method =>
                {
                    method.AddBracketScope(
                        $"return new global::DotNetCampus.Telescope.AttributedTypeMetadata<{baseTypeName}, {attributeTypeName}>[]",
                        "{",
                        "};",
                        block => block.AddRawStatements(items));
                });
            }
        });

        return builder.ToString();
    }
}
