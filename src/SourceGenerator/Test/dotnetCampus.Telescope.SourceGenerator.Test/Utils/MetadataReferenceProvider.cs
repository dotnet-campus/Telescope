using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGenerator.Test.Utils;

internal static class MetadataReferenceProvider
{
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
                metadataReferenceList.Add(MetadataReference.CreateFromFile(file));
            }
            catch
            {
                // 忽略
            }
        }

        _cacheList = metadataReferenceList;
        return _cacheList;
    }

    private static IReadOnlyList<MetadataReference>? _cacheList;
}