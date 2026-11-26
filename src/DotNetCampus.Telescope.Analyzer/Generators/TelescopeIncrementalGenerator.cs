using System.Collections.Immutable;
using System.Text;

using DotNetCampus.Telescope.Generators.CodeTransforming;
using DotNetCampus.Telescope.Generators.Models;
using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotNetCampus.Telescope.Generators;

/// <summary>
/// 响应程序集级 <c>[assembly: MarkExport(typeof(TBase), typeof(TAttribute))]</c> 特性并生成 <c>__AttributedTypesExport__</c> 的增量源生成器。
/// </summary>
[Generator(LanguageNames.CSharp)]
public class TelescopeIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 从编译程序集特性中读取所有 [assembly: MarkExport(typeof(Base), typeof(FooAttribute))] 声明。
        var markExportAttributesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                var markExportAttributeSymbol = compilation.GetTypeByMetadataName(InternalContracts.MarkExportAttributeMetadataName);
                if (markExportAttributeSymbol is null)
                {
                    return ImmutableArray<MarkExportAttributeParseResult>.Empty;
                }

                var builder = ImmutableArray.CreateBuilder<MarkExportAttributeParseResult>();
                foreach (var attributeData in compilation.Assembly.GetAttributes())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, markExportAttributeSymbol))
                    {
                        continue;
                    }

                    if (attributeData.ConstructorArguments.Length == 2
                        && attributeData.ConstructorArguments[0].Value is ITypeSymbol baseClassOrInterfaceType
                        && attributeData.ConstructorArguments[1].Value is ITypeSymbol attributeType)
                    {
                        builder.Add(new MarkExportAttributeParseResult(true, baseClassOrInterfaceType, attributeType));
                    }
                }

                return builder.ToImmutable();
            });

        // 遍历当前程序集中的所有非抽象非静态 class 定义，收集带有特性的候选类型。
        var assemblyClassIncrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (syntaxNode, _) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration),
                static (generatorSyntaxContext, cancellationToken) =>
                {
                    var classDeclarationSyntax = (ClassDeclarationSyntax)generatorSyntaxContext.Node;
                    var namedTypeSymbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);

                    if (namedTypeSymbol is not null
                        && !namedTypeSymbol.IsAbstract
                        && !namedTypeSymbol.IsStatic)
                    {
                        var attributes = namedTypeSymbol.GetAttributes();
                        if (attributes.Length > 0)
                        {
                            return new AssemblyCandidateClassParseResult(namedTypeSymbol, attributes, generatorSyntaxContext);
                        }
                    }

                    return new AssemblyCandidateClassParseResult();
                })
            .Where(static t => t.Success);

        // 将程序集特性和候选类型组合，筛选出所有符合导出条件的类型。
        IncrementalValueProvider<ImmutableArray<MarkClassParseResult>> collectionClass =
            assemblyClassIncrementalValuesProvider
                .Combine(markExportAttributesProvider)
                .Select(static (tuple, cancellationToken) => ParseMarkClassList(tuple.Left, tuple.Right, cancellationToken))
                .SelectMany(static (list, _) => list)
                .Collect();

        // 生成 __AttributedTypesExport__ 代码。
        IncrementalValueProvider<string> generatedCodeProvider = collectionClass.Select(static (markClassCollection, token) =>
        {
            var attributedTypesExportGenerator = new ExportedTypesCodeGenerator();
            return attributedTypesExportGenerator.Generate(markClassCollection, token);
        });

        context.RegisterSourceOutput(generatedCodeProvider,
            static (sourceProductionContext, generatedCode) =>
            {
                if (!string.IsNullOrEmpty(generatedCode))
                {
                    sourceProductionContext.AddSource(
                        $"{InternalContracts.AttributedTypesExportClassName}.g.cs",
                        SourceText.From(generatedCode, Encoding.UTF8));
                }
            });
    }

    private static List<MarkClassParseResult> ParseMarkClassList(
        AssemblyCandidateClassParseResult classParseResult,
        ImmutableArray<MarkExportAttributeParseResult> markExportAttributeParseResultList,
        CancellationToken cancellationToken)
    {
        var list = new List<MarkClassParseResult>();
        foreach (var markExportAttributeParseResult in markExportAttributeParseResultList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = ParseMarkClass(classParseResult, markExportAttributeParseResult);
            if (result is not null)
            {
                list.Add(result.Value);
            }
        }

        return list;
    }

    private static MarkClassParseResult? ParseMarkClass(
        AssemblyCandidateClassParseResult classParseResult,
        MarkExportAttributeParseResult markExportAttributeParseResult)
    {
        var matchAssemblyMarkAttributeData = classParseResult.Attributes.FirstOrDefault(t =>
            SymbolEqualityComparer.Default.Equals(t.AttributeClass, markExportAttributeParseResult.AttributeTypeInfo));

        if (matchAssemblyMarkAttributeData is null)
        {
            return default;
        }

        var requiredBaseClassOrInterfaceType = markExportAttributeParseResult.BaseClassOrInterfaceTypeInfo;
        if (classParseResult.ExportedTypeSymbol.IsInheritFrom(requiredBaseClassOrInterfaceType))
        {
            return new MarkClassParseResult(
                classParseResult.ExportedTypeSymbol,
                matchAssemblyMarkAttributeData,
                markExportAttributeParseResult);
        }

        return null;
    }
}
