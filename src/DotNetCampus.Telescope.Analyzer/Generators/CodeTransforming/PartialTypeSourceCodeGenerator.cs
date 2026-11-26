using System.Text;

using DotNetCampus.Telescope.Generators.Builders;
using DotNetCampus.Telescope.Utils.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.Telescope.Generators.CodeTransforming;

internal static class PartialTypeSourceCodeGenerator
{
    private static readonly SymbolDisplayFormat NamespaceDisplayFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    /// <summary>
    /// 使用 <see cref="SourceTextBuilder"/> 生成分部类型及其成员代码（支持顶级类型与嵌套类型）。
    /// </summary>
    public static string GeneratePartialTypeCode(
        INamedTypeSymbol partialClassType,
        Action<TypeDeclarationSourceTextBuilder> buildMembers)
    {
        var @namespace = partialClassType.ContainingNamespace is { IsGlobalNamespace: false } ns
            ? ns.ToDisplayString(NamespaceDisplayFormat)
            : null;

        if (!TryGetTypeDeclarationList(partialClassType, out var declarationList) || declarationList is null)
        {
            throw new ArgumentException($"无法为 {partialClassType} 创建代码");
        }

        // declarationList 为由内到外顺序，反转为由外到内。
        declarationList.Reverse();

        using var builder = new SourceTextBuilder(@namespace)
        {
            UseFileScopedNamespace = false,
            Nullable = null,
        };

        AddNestedTypeDeclarations(builder, declarationList, 0, buildMembers);
        return builder.ToString();
    }

    /// <summary>
    /// 构建分部方法的签名行（不含方法体）。
    /// </summary>
    public static string BuildPartialMethodSignature(IMethodSymbol partialMethodSymbol, CancellationToken token)
    {
        var signature = new StringBuilder();

        var accessibilityCode = partialMethodSymbol.DeclaredAccessibility.ToCSharpCode();
        if (!string.IsNullOrEmpty(accessibilityCode))
        {
            signature.Append(accessibilityCode).Append(' ');
        }

        if (partialMethodSymbol.IsStatic)
        {
            signature.Append("static ");
        }

        if (partialMethodSymbol.IsPartialDefinition)
        {
            signature.Append("partial ");
        }

        AppendTypeCode(partialMethodSymbol.ReturnType, signature, token);

        signature.Append(' ');
        signature.Append(partialMethodSymbol.Name);
        signature.Append("()"); // 暂时只支持无参函数

        return signature.ToString();
    }

    private static void AddNestedTypeDeclarations(
        IAllowTypeDeclaration container,
        List<string> declarations,
        int index,
        Action<TypeDeclarationSourceTextBuilder> buildInnermostMembers)
    {
        container.AddTypeDeclaration(declarations[index], type =>
        {
            if (index == declarations.Count - 1)
            {
                buildInnermostMembers(type);
            }
            else
            {
                AddNestedTypeDeclarations(type, declarations, index + 1, buildInnermostMembers);
            }
        });
    }

    private static void AppendTypeCode(ITypeSymbol typeSymbol, StringBuilder output, CancellationToken token)
    {
        var returnTypeCode = typeSymbol.ToGlobalNameWithoutGenerics();
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

                        AppendTypeCode(info.ItemType, output, token);

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
                    AppendTypeCode(typeArgument, output, token);
                }

                if (i != returnTypeNamedTypeSymbol.TypeArguments.Length - 1)
                {
                    output.Append(", ");
                }
            }

            output.Append('>');
        }
    }

    private static bool TryGetTypeDeclarationList(INamedTypeSymbol typeSymbol, out List<string>? classDeclarationList)
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
}
