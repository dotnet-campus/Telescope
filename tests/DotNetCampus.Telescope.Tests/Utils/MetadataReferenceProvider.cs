using System.Reflection;

using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Tests.Utils;

internal static class MetadataReferenceProvider
{
    private static IReadOnlyList<MetadataReference>? _cacheList;

    public static IReadOnlyList<MetadataReference> GetDotNetMetadataReferenceList()
    {
        if (_cacheList is not null)
        {
            return _cacheList;
        }

        var metadataReferenceList = new List<MetadataReference>();
        var assembly = Assembly.Load("System.Runtime");
        foreach (var file in Directory.GetFiles(Path.GetDirectoryName(assembly.Location)!, "*.dll"))
        {
            try
            {
                // 仅加载包含托管元数据的程序集，跳过原生 DLL（如 msquic.dll、mscordbi.dll）。
                _ = AssemblyName.GetAssemblyName(file);
                metadataReferenceList.Add(MetadataReference.CreateFromFile(file));
            }
            catch
            {
                // 忽略非托管 DLL
            }
        }

        _cacheList = metadataReferenceList;
        return _cacheList;
    }
}
