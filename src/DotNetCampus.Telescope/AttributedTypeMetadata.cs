using System;
using System.Diagnostics;

namespace DotNetCampus.Telescope;

/// <summary>
/// 表示在编译期导出的类型及其特性与实例创建器的元数据。
/// </summary>
/// <typeparam name="TBaseClassOrInterface">导出类型的基类或实现的接口类型。</typeparam>
/// <typeparam name="TAttribute">导出类型上标记的特性类型。</typeparam>
[DebuggerDisplay("AttributedTypeMetadata<{RealType}, {Attribute}>")]
public class AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>(
    Type realType,
    TAttribute attribute,
    Func<TBaseClassOrInterface> instanceCreator)
    where TAttribute : Attribute
{
    private readonly Func<TBaseClassOrInterface> _instanceCreator =
        instanceCreator ?? throw new ArgumentNullException(nameof(instanceCreator));

    /// <summary>
    /// 获取被导出的实际类型。
    /// </summary>
    public Type RealType { get; } = realType ?? throw new ArgumentNullException(nameof(realType));

    /// <summary>
    /// 获取标记在该类型上的特性实例。
    /// </summary>
    public TAttribute Attribute { get; } = attribute ?? throw new ArgumentNullException(nameof(attribute));

    /// <summary>
    /// 创建该类型的新实例。
    /// </summary>
    /// <returns>创建的实例。</returns>
    public TBaseClassOrInterface CreateInstance() => _instanceCreator();
}
