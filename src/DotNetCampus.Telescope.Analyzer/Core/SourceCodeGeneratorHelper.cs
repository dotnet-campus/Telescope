using System.Diagnostics;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal static class SourceCodeGeneratorHelper
{
    /// <summary>
    /// 创建分部方法的代码。
    /// </summary>
    public static string GeneratePartialMethodCode(IMethodSymbol partialMethodSymbol, string methodCode, CancellationToken token)
    {
        var methodSource = new StringBuilder();

        var accessibilityCode = partialMethodSymbol.DeclaredAccessibility.ToCSharpCode();
        if (!string.IsNullOrEmpty(accessibilityCode))
        {
            methodSource.Append(accessibilityCode).Append(' ');
        }

        if (partialMethodSymbol.IsStatic)
        {
            methodSource.Append("static ");
        }

        if (partialMethodSymbol.IsPartialDefinition)
        {
            methodSource.Append("partial ");
        }

        var returnType = partialMethodSymbol.ReturnType;
        GenerateTypeCode(returnType, methodSource, token);

        methodSource.Append(' ');
        methodSource.Append(partialMethodSymbol.Name);
        methodSource.AppendLine("()"); // 暂时只支持无参函数
        methodSource.AppendLine("{");
        methodSource.AppendLine(IndentSource(methodCode, 1, shouldFirstLine: true));
        methodSource.Append('}');

        return methodSource.ToString();
    }

    /// <summary>
    /// 根据传入的类型创建代码。
    /// </summary>
    public static void GenerateTypeCode(ITypeSymbol typeSymbol, StringBuilder output, CancellationToken token)
    {
        var returnTypeCode = TypeSymbolHelper.TypeSymbolToFullName(typeSymbol);
        output.Append(returnTypeCode);
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } returnTypeNamedTypeSymbol)
        {
            output.Append('<');
            for (var i = 0; i < returnTypeNamedTypeSymbol.TypeArguments.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var typeArgument = returnTypeNamedTypeSymbol.TypeArguments[i];

                if (typeArgument.IsTupleType
                    && ValueTupleInfoParser.TryParse(typeArgument, token, out var valueTupleInfo))
                {
                    output.Append('(');
                    for (var index = 0; index < valueTupleInfo.ItemList.Count; index++)
                    {
                        var info = valueTupleInfo.ItemList[index];

                        GenerateTypeCode(info.ItemType, output, token);

                        if (!string.IsNullOrEmpty(info.ItemName))
                        {
                            output.Append(' ').Append(info.ItemName);
                        }

                        if (index != valueTupleInfo.ItemList.Count - 1)
                        {
                            output.Append(", ");
                        }
                    }

                    output.Append(')');
                }
                else
                {
                    GenerateTypeCode(typeArgument, output, token);
                }

                if (i != returnTypeNamedTypeSymbol.TypeArguments.Length - 1)
                {
                    output.Append(", ");
                }
            }

            output.Append('>');
        }
    }

    /// <summary>
    /// 创建 partial 类型的代码（支持顶级类型与嵌套类型）。
    /// </summary>
    public static string GeneratePartialClassCode(INamedTypeSymbol partialClassType, string memberCode)
    {
        var symbolDisplayFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        var @namespace = partialClassType.ContainingNamespace is { IsGlobalNamespace: false } ns
            ? ns.ToDisplayString(symbolDisplayFormat)
            : string.Empty;

        if (!TryGetClassDeclarationList(partialClassType, out var declarationList) || declarationList is null)
        {
            throw new ArgumentException($"无法为 {partialClassType} 创建代码");
        }

        // declarationList 为由内到外顺序，将其反转为由外到内。
        declarationList.Reverse();

        var isIncludeNamespace = !string.IsNullOrEmpty(@namespace);
        var stringBuilder = new StringBuilder(AssemblyInfo.GeneratedCodeComment);

        var currentIndent = 0;
        if (isIncludeNamespace)
        {
            stringBuilder.AppendLine($"namespace {@namespace}");
            stringBuilder.AppendLine("{");
            currentIndent = 1;
        }

        foreach (var declaration in declarationList)
        {
            var indent = new string(' ', 4 * currentIndent);
            stringBuilder.Append(indent).AppendLine(declaration);
            stringBuilder.Append(indent).AppendLine("{");
            currentIndent++;
        }

        stringBuilder.AppendLine(IndentSource(memberCode, currentIndent, shouldFirstLine: true));

        for (var i = declarationList.Count - 1; i >= 0; i--)
        {
            currentIndent--;
            var indent = new string(' ', 4 * currentIndent);
            stringBuilder.Append(indent).AppendLine("}");
        }

        if (isIncludeNamespace)
        {
            stringBuilder.AppendLine("}");
        }

        return stringBuilder.ToString();
    }

    private static bool TryGetClassDeclarationList(INamedTypeSymbol typeSymbol, out List<string>? classDeclarationList)
    {
        INamedTypeSymbol? currentSymbol = typeSymbol;
        classDeclarationList = null;

        while (currentSymbol is not null)
        {
            if (currentSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                SyntaxTokenList tokenList = typeDeclarationSyntax.Modifiers;
                int tokenCount = tokenList.Count;

                bool isPartial = false;
                string[] declarationElements = new string[tokenCount + 2];

                for (int i = 0; i < tokenCount; i++)
                {
                    SyntaxToken token = tokenList[i];
                    declarationElements[i] = token.Text;

                    if (token.IsKind(SyntaxKind.PartialKeyword))
                    {
                        isPartial = true;
                    }
                }

                if (!isPartial)
                {
                    classDeclarationList = null;
                    return false;
                }

                declarationElements[tokenCount] = typeDeclarationSyntax.Keyword.Text;
                declarationElements[tokenCount + 1] = currentSymbol.Name;

                (classDeclarationList ??= []).Add(string.Join(" ", declarationElements));
            }

            currentSymbol = currentSymbol.ContainingType;
        }

        return classDeclarationList is { Count: > 0 };
    }

    /// <summary>
    /// 提供缩进的方法。
    /// </summary>
    public static string IndentSource(string source, int numIndentations, bool shouldFirstLine = false)
    {
        Debug.Assert(numIndentations >= 0);
        if (numIndentations == 0)
        {
            return source;
        }

        var indent = new string(' ', 4 * numIndentations);
        var result = source.Replace("\r", "").Replace("\n", $"\r\n{indent}");

        if (shouldFirstLine)
        {
            result = indent + result;
        }

        return result;
    }
}
