using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetCampus.Telescope.Analyzer.Core;

internal static class AttributeCodeReWriter
{
    /// <summary>
    /// 从 <paramref name="attributeData"/> 转换为特性生成代码。从 <c>[Foo(xx, xxx)]</c> 语义转换为 <c>new Foo(xx, xxx)</c> 的生成代码。
    /// </summary>
    public static string GetAttributeCreatedCode(AttributeData attributeData)
    {
        var constructorArgumentCodeList = new List<string>(attributeData.ConstructorArguments.Length);
        foreach (TypedConstant constructorArgument in attributeData.ConstructorArguments)
        {
            constructorArgumentCodeList.Add(TypedConstantToCodeString(constructorArgument));
        }

        var namedArgumentCodeList = new List<(string PropertyName, string ValueCode)>(attributeData.NamedArguments.Length);
        foreach (var keyValuePair in attributeData.NamedArguments)
        {
            namedArgumentCodeList.Add((keyValuePair.Key, TypedConstantToCodeString(keyValuePair.Value)));
        }

        var attributeFullName = TypeSymbolHelper.TypeSymbolToFullName(attributeData.AttributeClass!);
        var constructorArgs = string.Join(", ", constructorArgumentCodeList);

        if (namedArgumentCodeList.Count == 0)
        {
            return $"new {attributeFullName}({constructorArgs})";
        }

        var namedArgs = string.Join(",\n    ", namedArgumentCodeList.Select(static x => $"{x.PropertyName} = {x.ValueCode}"));
        return $$"""
            new {{attributeFullName}}({{constructorArgs}})
            {
                {{namedArgs}}
            }
            """;

        static string TypedConstantToCodeString(TypedConstant typedConstant)
        {
            if (typedConstant.IsNull)
            {
                return "null";
            }

            switch (typedConstant.Kind)
            {
                case TypedConstantKind.Enum:
                {
                    var enumTypeName = TypeSymbolHelper.TypeSymbolToFullName(typedConstant.Type!);
                    var valueText = typedConstant.Value?.ToString() ?? "0";
                    return valueText.StartsWith("-", StringComparison.Ordinal)
                        ? $"({enumTypeName})({valueText})"
                        : $"({enumTypeName}){valueText}";
                }
                case TypedConstantKind.Type:
                {
                    return typedConstant.Value is ITypeSymbol typeSymbol
                        ? $"typeof({TypeSymbolHelper.TypeSymbolToFullName(typeSymbol)})"
                        : "null";
                }
                case TypedConstantKind.Primitive:
                {
                    return typedConstant.Value is null
                        ? "null"
                        : SymbolDisplay.FormatPrimitive(typedConstant.Value, quoteStrings: true, useHexadecimalNumbers: false);
                }
                case TypedConstantKind.Array:
                {
                    var elements = string.Join(", ", typedConstant.Values.Select(TypedConstantToCodeString));
                    if (typedConstant.Type is IArrayTypeSymbol arrayTypeSymbol)
                    {
                        var elementTypeName = arrayTypeSymbol.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        return $"new {elementTypeName}[] {{ {elements} }}";
                    }

                    return $"new object[] {{ {elements} }}";
                }
                default:
                {
                    return typedConstant.Value?.ToString() ?? "null";
                }
            }
        }
    }
}
