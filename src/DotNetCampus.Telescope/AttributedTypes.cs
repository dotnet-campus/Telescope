using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

using static DotNetCampus.Telescope.InternalContracts;

namespace DotNetCampus.Telescope;

/// <summary>
/// 当某程序集通过 DotNetCampus.Telescope 源生成器编译后，可通过此类型高性能地查找类型并创建类型实例。
/// </summary>
public static class AttributedTypes
{
    /// <summary>
    /// 从指定的程序集 <paramref name="assemblies"/> 中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
    /// </summary>
    /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
    /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
    /// <param name="assemblies">要查找的程序集（或程序集集合）。</param>
    /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
    public static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> FromAssembly<TBaseClassOrInterface, TAttribute>(params Assembly[] assemblies)
        where TAttribute : Attribute
        => FromAssembly<TBaseClassOrInterface, TAttribute>((IEnumerable<Assembly>)assemblies);

    /// <summary>
    /// 从指定的程序集 <paramref name="assemblies"/> 中查找标记了 <typeparamref name="TAttribute"/> 的 <typeparamref name="TBaseClassOrInterface"/> 的子类（含自身）或接口的实现类。
    /// </summary>
    /// <typeparam name="TBaseClassOrInterface">要查找的类型必须是此类型的子类（含自身）或此接口的实现类。如果任意类型均可，请使用 <see cref="object"/>。</typeparam>
    /// <typeparam name="TAttribute">要查找的类型必须已标记的特性（<see cref="Attribute"/>）类型。</typeparam>
    /// <param name="assemblies">要查找的程序集（或程序集集合）。</param>
    /// <returns>对这些程序集集合中所有已导出的指定类型的元数据的集合。</returns>
    public static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> FromAssembly<TBaseClassOrInterface, TAttribute>(IEnumerable<Assembly> assemblies)
        where TAttribute : Attribute
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        List<Exception>? exceptions = null;
        foreach (var assembly in assemblies)
        {
            IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>>? metadataList = null;
            try
            {
                metadataList = LoadFromAssembly<TBaseClassOrInterface, TAttribute>(assembly);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }

            if (metadataList is not null)
            {
                foreach (var metadata in metadataList)
                {
                    yield return metadata;
                }
            }
        }

        if (exceptions is { Count: > 0 })
        {
            ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
        }
    }

    private static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> LoadFromAssembly<TBaseClassOrInterface, TAttribute>(Assembly assembly)
        where TAttribute : Attribute
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var exportType = assembly.GetType(AttributedTypesExportFullClassName);
        return exportType is not null
            && Activator.CreateInstance(exportType) is ICompileTimeAttributedTypesExporter<TBaseClassOrInterface, TAttribute> exporter
            ? LoadFromExporter(exporter)
            : Enumerable.Empty<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>>();
    }

    private static IEnumerable<AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>> LoadFromExporter<TBaseClassOrInterface, TAttribute>(
        ICompileTimeAttributedTypesExporter<TBaseClassOrInterface, TAttribute> exporter)
        where TAttribute : Attribute
        => exporter.ExportAttributeTypes();
}
