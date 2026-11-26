using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.Telescope.Analyzer;

/// <summary>
/// 程序集里可能被标记导出的类型。
/// </summary>
internal readonly struct AssemblyCandidateClassParseResult
{
    public AssemblyCandidateClassParseResult(
        INamedTypeSymbol exportedTypeSymbol,
        ImmutableArray<AttributeData> attributes,
        GeneratorSyntaxContext generatorSyntaxContext)
    {
        ExportedTypeSymbol = exportedTypeSymbol;
        Attributes = attributes;
        GeneratorSyntaxContext = generatorSyntaxContext;
        Success = true;
    }

    public AssemblyCandidateClassParseResult()
    {
        Success = false;
        ExportedTypeSymbol = default!;
        Attributes = default;
        GeneratorSyntaxContext = default;
    }

    public INamedTypeSymbol ExportedTypeSymbol { get; }

    public ClassDeclarationSyntax ExportedTypeClassDeclarationSyntax => (ClassDeclarationSyntax)GeneratorSyntaxContext.Node;

    public ImmutableArray<AttributeData> Attributes { get; }

    public GeneratorSyntaxContext GeneratorSyntaxContext { get; }

    public bool Success { get; }
}
