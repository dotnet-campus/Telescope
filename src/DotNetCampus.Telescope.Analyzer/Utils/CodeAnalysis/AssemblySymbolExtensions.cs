using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Utils.CodeAnalysis;

internal static class AssemblySymbolExtensions
{
    /// <summary>
    /// 获取当前程序集里面的所有类型。
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> GetAllTypeSymbols(this IAssemblySymbol assemblySymbol) =>
        GetAllTypeSymbols(assemblySymbol.GlobalNamespace);

    public static IEnumerable<INamedTypeSymbol> GetAllTypeSymbols(this INamespaceSymbol namespaceSymbol)
    {
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            yield return typeSymbol;
        }

        foreach (var namespaceMember in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var typeSymbol in GetAllTypeSymbols(namespaceMember))
            {
                yield return typeSymbol;
            }
        }
    }

    public static bool IsReference(this IAssemblySymbol currentAssemblySymbol, IAssemblySymbol requiredAssemblySymbol)
    {
        var visited = new Dictionary<IAssemblySymbol, bool>(SymbolEqualityComparer.Default);
        return IsReference(currentAssemblySymbol, requiredAssemblySymbol, visited);
    }

    public static bool IsReference(
        this IAssemblySymbol currentAssemblySymbol,
        IAssemblySymbol requiredAssemblySymbol,
        Dictionary<IAssemblySymbol, bool> visited)
    {
        if (SymbolEqualityComparer.Default.Equals(currentAssemblySymbol, requiredAssemblySymbol))
        {
            return true;
        }

        foreach (var moduleSymbol in currentAssemblySymbol.Modules)
        {
            foreach (var referencedAssemblySymbol in moduleSymbol.ReferencedAssemblySymbols)
            {
                if (SymbolEqualityComparer.Default.Equals(referencedAssemblySymbol, requiredAssemblySymbol))
                {
                    visited[currentAssemblySymbol] = true;
                    return true;
                }

                if (SymbolEqualityComparer.Default.Equals(referencedAssemblySymbol, currentAssemblySymbol))
                {
                    continue;
                }

                if (!visited.TryGetValue(referencedAssemblySymbol, out var isReference))
                {
                    visited[referencedAssemblySymbol] = false;
                    isReference = IsReference(referencedAssemblySymbol, requiredAssemblySymbol, visited);
                    visited[referencedAssemblySymbol] = isReference;
                }

                if (isReference)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
