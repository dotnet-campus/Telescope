using System.Collections.Immutable;
using System.Text;

using DotNetCampus.Telescope.Generators.CodeTransforming;
using DotNetCampus.Telescope.Generators.Models;
using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static DotNetCampus.Telescope.Properties.Resources;

namespace DotNetCampus.Telescope.Generators;

/// <summary>
/// 从标记了 <c>[TelescopeExport]</c> 的分部方法导出类型的增量源生成器。
/// </summary>
// 形如 private static partial IEnumerable<(Type type, FooAttribute attribute, Func<FooBase> creator)> ExportFooEnumerable();
[Generator(LanguageNames.CSharp)]
public class TelescopeExportTypeToMethodIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 使用 ForAttributeWithMetadataName 快速定位标记了 TelescopeExportAttribute 的分部方法。
        IncrementalValuesProvider<ExportTypeCollectionResult> exportMethodIncrementalValuesProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                InternalContracts.TelescopeExportAttributeMetadataName,
                static (syntaxNode, _) =>
                    syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
                    && methodDeclarationSyntax.Modifiers.Any(static t => t.IsKind(SyntaxKind.PartialKeyword)),
                static (generatorAttributeSyntaxContext, token) =>
                {
                    token.ThrowIfCancellationRequested();

                    if (generatorAttributeSyntaxContext.TargetSymbol is not IMethodSymbol methodSymbol)
                    {
                        return null;
                    }

                    var attributeData = generatorAttributeSyntaxContext.Attributes.FirstOrDefault();
                    if (attributeData is null)
                    {
                        return null;
                    }

                    var includeReference = attributeData.NamedArguments
                        .FirstOrDefault(static t => t.Key == InternalContracts.TelescopeExportIncludeReferencesPropertyName)
                        .Value.Value is true;

                    return new ExportTypeCollectionResult(methodSymbol, generatorAttributeSyntaxContext.TargetNode)
                    {
                        IncludeReference = includeReference,
                    };
                })
            .ExcludeNulls();

        // 解析导出方法的返回值签名。
        var exportMethodReturnTypeCollectionResultIncrementalValuesProvider = exportMethodIncrementalValuesProvider.Select(static (exportTypeCollectionResult, token) =>
        {
            ITypeSymbol methodSymbolReturnType = exportTypeCollectionResult.ExportPartialMethodSymbol.ReturnType;

            if (methodSymbolReturnType is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.IsGenericType
                    && namedTypeSymbol.ToGlobalNameWithoutGenerics() == "global::System.Collections.Generic.IEnumerable")
                {
                    // 要求符合以下定义之一：
                    // static partial IEnumerable<(Type, FooAttribute xx, Func<Base> xxx)> ExportFooEnumerable()
                    // static partial IEnumerable<(Type, Func<Base> xxx)> ExportFooEnumerable()
                    if (namedTypeSymbol.TypeArguments.Length == 1
                        && namedTypeSymbol.TypeArguments[0] is INamedTypeSymbol tupleType
                        && tupleType.IsTupleType
                        && tupleType.TupleElements.Length > 0)
                    {
                        if (tupleType.TupleElements.Length == 3)
                        {
                            if (tupleType.TupleElements[0].Type.ToGlobalNameWithoutGenerics() != "global::System.Type")
                            {
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            var expectedClassAttributeType = tupleType.TupleElements[1].Type;

                            if (tupleType.TupleElements[2].Type is not INamedTypeSymbol funcTypeSymbol
                                || !funcTypeSymbol.IsGenericType
                                || funcTypeSymbol.ToGlobalNameWithoutGenerics() != "global::System.Func")
                            {
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            var expectedClassBaseType = funcTypeSymbol.TypeArguments[0];

                            return new ExportMethodReturnTypeCollectionResult(
                                expectedClassBaseType,
                                expectedClassAttributeType,
                                exportTypeCollectionResult,
                                ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator);
                        }
                        else if (tupleType.TupleElements.Length == 2)
                        {
                            if (tupleType.TupleElements[0].Type.ToGlobalNameWithoutGenerics() != "global::System.Type")
                            {
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            if (tupleType.TupleElements[1].Type is not INamedTypeSymbol funcTypeSymbol
                                || !funcTypeSymbol.IsGenericType
                                || funcTypeSymbol.ToGlobalNameWithoutGenerics() != "global::System.Func")
                            {
                                return ReturnTypeError(nameof(Tes001_Message_EnumerableValueTupleWithTypeAttributeCreator));
                            }

                            var expectedClassBaseType = funcTypeSymbol.TypeArguments[0];

                            return new ExportMethodReturnTypeCollectionResult(
                                expectedClassBaseType,
                                expectedClassAttributeType: null,
                                exportTypeCollectionResult,
                                ExportMethodReturnType.EnumerableValueTupleWithTypeAttributeCreator);
                        }
                    }
                }
            }

            return (IExportMethodReturnTypeCollectionResult)new ExportMethodReturnTypeCollectionDiagnostic(
                Diagnostics.Tes000_UnknownError,
                GetLocation());

            Location GetLocation()
            {
                var syntaxNode = exportTypeCollectionResult.TargetSyntaxNode;
                return Location.Create(syntaxNode.SyntaxTree, syntaxNode.Span);
            }

            ExportMethodReturnTypeCollectionDiagnostic ReturnTypeError(string messageKey)
            {
                return new ExportMethodReturnTypeCollectionDiagnostic(
                    Diagnostics.Tes001_MethodReturnTypeError,
                    GetLocation(),
                    Diagnostics.Localize(messageKey));
            }
        });

        // 报告方法签名诊断错误。
        var diagnosticIncrementalValuesProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select(static (t, _) => t as ExportMethodReturnTypeCollectionDiagnostic)
            .ExcludeNulls();

        context.RegisterSourceOutput(diagnosticIncrementalValuesProvider, static (productionContext, diagnostic) =>
        {
            productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
        });

        // 收集需要扫描引用程序集的导出方法。
        var assemblyReferenceExportReturnTypeProvider = exportMethodReturnTypeCollectionResultIncrementalValuesProvider
            .Select(static (t, _) => t as ExportMethodReturnTypeCollectionResult)
            .Where(static t => t is not null && t.ExportTypeCollectionResult.IncludeReference)
            .Select(static (t, _) => t!)
            .Collect();

        // 扫描引用的程序集中的候选类型。
        var referenceAssemblyTypeIncrementalValueProvider = context.CompilationProvider
            .Select(static (compilation, _) => compilation.SourceModule.ReferencedAssemblySymbols)
            .Combine(assemblyReferenceExportReturnTypeProvider)
            .Select(static (tuple, token) =>
            {
                var referencedAssemblySymbols = tuple.Left;
                var exportMethodReturnTypeCollectionResults = tuple.Right;

                var candidateClassList = new List<CandidateClassTypeResult>();

                foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResults)
                {
                    token.ThrowIfCancellationRequested();

                    var assemblyClassTypeSymbolList = new List<INamedTypeSymbol>();
                    var candidateClassTypeResult = new CandidateClassTypeResult(exportMethodReturnTypeCollectionResult, assemblyClassTypeSymbolList);
                    candidateClassList.Add(candidateClassTypeResult);

                    var expectedClassBaseType = exportMethodReturnTypeCollectionResult.ExpectedClassBaseType;
                    var currentAssembly = exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.ContainingAssembly;
                    var visited = new Dictionary<IAssemblySymbol, bool>(SymbolEqualityComparer.Default);

                    foreach (var referencedAssemblySymbol in referencedAssemblySymbols)
                    {
                        token.ThrowIfCancellationRequested();

                        if (!referencedAssemblySymbol.IsReference(expectedClassBaseType.ContainingAssembly, visited))
                        {
                            continue;
                        }

                        var isInternalsVisibleTo = referencedAssemblySymbol.GivesAccessTo(currentAssembly);

                        foreach (var assemblyClassTypeSymbol in referencedAssemblySymbol.GetAllTypeSymbols())
                        {
                            if (!isInternalsVisibleTo
                                && assemblyClassTypeSymbol.DeclaredAccessibility != Accessibility.Public)
                            {
                                continue;
                            }

                            if (exportMethodReturnTypeCollectionResult.IsMatch(assemblyClassTypeSymbol))
                            {
                                assemblyClassTypeSymbolList.Add(assemblyClassTypeSymbol);
                            }
                        }
                    }
                }

                return new CandidateClassCollectionResult(candidateClassList);
            });

        // 收集所有有效的导出方法定义，准备扫描当前程序集。
        IncrementalValueProvider<ImmutableArray<ExportMethodReturnTypeCollectionResult>> returnTypeCollectionIncrementalValuesProvider =
            exportMethodReturnTypeCollectionResultIncrementalValuesProvider
                .Select(static (t, _) => t as ExportMethodReturnTypeCollectionResult)
                .ExcludeNulls()
                .Collect();

        // 扫描当前程序集中的所有非抽象非静态类。
        var candidateClassCollectionResultIncrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (syntaxNode, _) => syntaxNode.IsKind(SyntaxKind.ClassDeclaration),
                static (generatorSyntaxContext, token) =>
                {
                    var classDeclarationSyntax = (ClassDeclarationSyntax)generatorSyntaxContext.Node;
                    var assemblyClassTypeSymbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
                    if (assemblyClassTypeSymbol is not null
                        && !assemblyClassTypeSymbol.IsAbstract
                        && !assemblyClassTypeSymbol.IsStatic)
                    {
                        return assemblyClassTypeSymbol;
                    }

                    return null;
                })
            .ExcludeNulls()
            .Combine(returnTypeCollectionIncrementalValuesProvider)
            .Select(static (tuple, token) =>
            {
                var assemblyClassTypeSymbol = tuple.Left;
                var exportMethodReturnTypeCollectionResultArray = tuple.Right;

                var result = new List<CandidateClassTypeResult>();

                foreach (var exportMethodReturnTypeCollectionResult in exportMethodReturnTypeCollectionResultArray)
                {
                    token.ThrowIfCancellationRequested();
                    if (exportMethodReturnTypeCollectionResult.IsMatch(assemblyClassTypeSymbol))
                    {
                        result.Add(new CandidateClassTypeResult(
                            exportMethodReturnTypeCollectionResult,
                            [assemblyClassTypeSymbol]));
                    }
                }

                return result.Count > 0 ? new CandidateClassCollectionResult(result) : null;
            })
            .ExcludeNulls();

        // 合并当前程序集与引用程序集的结果，同时保留所有合法的导出方法（即便匹配到 0 个类型也要生成方法实现，避免 CS8795）。
        var collectionResultIncrementalValueProvider = returnTypeCollectionIncrementalValuesProvider
            .Combine(referenceAssemblyTypeIncrementalValueProvider)
            .Combine(candidateClassCollectionResultIncrementalValuesProvider.Collect())
            .Select(static (tuple, token) =>
            {
                var allExportMethods = tuple.Left.Left;
                var referencedCandidates = tuple.Left.Right;
                var localCandidates = tuple.Right;

                var dictionary = new Dictionary<ExportMethodReturnTypeCollectionResult, List<INamedTypeSymbol>>();
                var seenSets = new Dictionary<ExportMethodReturnTypeCollectionResult, HashSet<INamedTypeSymbol>>();

                foreach (var exportMethod in allExportMethods)
                {
                    token.ThrowIfCancellationRequested();
                    if (!dictionary.ContainsKey(exportMethod))
                    {
                        dictionary.Add(exportMethod, []);
                        seenSets.Add(exportMethod, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
                    }
                }

                void AddCandidates(CandidateClassCollectionResult candidateCollection)
                {
                    foreach (var candidateClassTypeResult in candidateCollection.CandidateClassTypeResultList)
                    {
                        token.ThrowIfCancellationRequested();
                        var key = candidateClassTypeResult.ExportMethodReturnTypeCollectionResult;
                        if (!dictionary.TryGetValue(key, out var list))
                        {
                            list = [];
                            dictionary.Add(key, list);
                            seenSets[key] = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                        }

                        var seen = seenSets[key];
                        foreach (var symbol in candidateClassTypeResult.AssemblyClassTypeSymbolList)
                        {
                            if (seen.Add(symbol))
                            {
                                list.Add(symbol);
                            }
                        }
                    }
                }

                foreach (var localCandidate in localCandidates)
                {
                    AddCandidates(localCandidate);
                }

                AddCandidates(referencedCandidates);

                return dictionary;
            });

        // 在 Select 中通过 SourceTextBuilder 生成源码以便支持增量取消。
        var sourceCodeProvider = collectionResultIncrementalValueProvider.Select(static (result, token) =>
        {
            var codeList = new List<(string FileName, string SourceCode)>(result.Count);
            foreach (var item in result)
            {
                token.ThrowIfCancellationRequested();

                var exportMethodReturnTypeCollectionResult = item.Key;
                var list = item.Value;
                var partialClassType = exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.ContainingType;

                var code = PartialTypeSourceCodeGenerator.GeneratePartialTypeCode(
                    partialClassType,
                    type => ExportMethodCodeGenerator.BuildMethod(type, exportMethodReturnTypeCollectionResult, list, token));

                var fileName = $"{partialClassType.Name}.{exportMethodReturnTypeCollectionResult.ExportPartialMethodSymbol.Name}.g.cs";
                codeList.Add((fileName, code));
            }

            return codeList;
        });

        context.RegisterImplementationSourceOutput(sourceCodeProvider,
            static (productionContext, result) =>
            {
                foreach (var (fileName, code) in result)
                {
                    productionContext.AddSource(fileName, SourceText.From(code, Encoding.UTF8));
                }
            });
    }
}
