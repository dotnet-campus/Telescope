using System;

namespace DotNetCampus.Telescope;

/// <summary>
/// 编译时，如果一个类型的基类或实现的接口是 <paramref name="baseClassOrInterfaceType"/> 类型，且标记了 <paramref name="attributeType"/> 特性，则将其导出。
/// <para>标记导出后，在运行时，可通过 <see cref="AttributedTypes.FromAssembly{TBaseClassOrInterface, TAttribute}(System.Reflection.Assembly[])"/> 将导出的这些类型实例化。</para>
/// <para>此操作用来替代一部分的反射，用以解决应用程序启动时因反射导致的耗时。</para>
/// </summary>
/// <param name="baseClassOrInterfaceType">标记导出此程序集中的此类型的子类（含自身）或者此接口的实现类的类型。如果任意类型均可，请使用 typeof(<see cref="object"/>)。</param>
/// <param name="attributeType">类型上标记的特性（<see cref="Attribute"/>）类型。</param>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class MarkExportAttribute(Type baseClassOrInterfaceType, Type attributeType) : Attribute
{
    /// <summary>
    /// 获取要求导出类型继承的基类或实现的接口类型。
    /// </summary>
    public Type RealType { get; } = baseClassOrInterfaceType;

    /// <summary>
    /// 获取要求导出类型标记的特性类型。
    /// </summary>
    public Type AttributeType { get; } = attributeType;
}
