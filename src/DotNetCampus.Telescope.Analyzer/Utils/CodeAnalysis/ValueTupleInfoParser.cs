using System.Diagnostics;

using DotNetCampus.Telescope.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.Telescope.Utils.CodeAnalysis;

internal static class ValueTupleInfoParser
{
    public static bool TryParse(ITypeSymbol type, CancellationToken token, out ValueTupleInfo valueTupleInfo)
    {
        valueTupleInfo = null!;

        if (type is not INamedTypeSymbol typeArgument)
        {
            return false;
        }

        // 尝试判断是 ValueTuple 的情况：(Type type, FooAttribute xx, Func<Base> xxx)
        if (type.IsValueType
            && typeArgument.TupleElements.Length > 0
            && typeArgument.DeclaringSyntaxReferences.Length > 0
            && typeArgument.DeclaringSyntaxReferences[0].GetSyntax(token) is TupleTypeSyntax valueTupleSyntaxNode)
        {
            Debug.Assert(typeArgument.TupleElements.Length == valueTupleSyntaxNode.Elements.Count);

            var list = new List<ValueTupleItemSyntaxAndSymbolInfo>(typeArgument.TupleElements.Length);

            for (var i = 0; i < valueTupleSyntaxNode.Elements.Count; i++)
            {
                var tupleItemTypeSymbol = typeArgument.TupleElements[i];
                var tupleItemSyntax = valueTupleSyntaxNode.Elements[i];
                var name = tupleItemSyntax.Identifier.Text;

                list.Add(new ValueTupleItemSyntaxAndSymbolInfo(tupleItemTypeSymbol.Type, name));
            }

            valueTupleInfo = new ValueTupleInfo(list);
            return true;
        }

        return false;
    }
}
