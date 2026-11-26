# Telescope

Telescope 是一套基于 Roslyn 增量源生成器的预编译框架。

|[中文](README.md)|[English](docs/en-us/README.md)|
|-|-|

当项目安装 `DotNetCampus.Telescope` NuGet 包后，即可在编译期自动扫描并导出程序集中的类型与特性元数据，将耗时的运行时反射迁移到编译期执行，大幅度提升应用程序启动与运行性能。

[![.NET Build](https://github.com/dotnet-campus/Telescope/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/dotnet-campus/Telescope/actions/workflows/dotnet-build.yml)

|Package|NuGet|
|--|--|
|DotNetCampus.Telescope|[![NuGet](https://img.shields.io/nuget/v/DotNetCampus.Telescope.svg)](https://www.nuget.org/packages/DotNetCampus.Telescope)|

支持的目标框架：`net6.0`、`netstandard2.0`、`net45`。

## 安装

```xml
<PackageReference Include="DotNetCampus.Telescope" Version="1.0.0" />
```

`DotNetCampus.Telescope` 单个 NuGet 包同时包含运行时类型与 Roslyn 增量源生成器，不再需要分别安装 `TelescopeSource` 或独立的分析器包。

## 用法

支持两种导出写法：

### 1. 分部方法式（推荐）

在分部类里定义分部方法，标记 `[TelescopeExport]` 特性，并在返回值元组中声明导出条件：

```csharp
using DotNetCampus.Telescope;

internal partial class Program
{
    [TelescopeExport]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

以上代码会在编译期导出当前项目中标记了 `F1Attribute` 且继承自 `Base` 的所有非抽象类型，源生成器会自动补全该分部方法的实现：

```csharp
internal partial class Program
{
    private static partial global::System.Collections.Generic.IEnumerable<(global::System.Type type, global::Demo.F1Attribute attribute, global::System.Func<global::Demo.Base> creator)> ExportFooEnumerable()
    {
        yield return (typeof(global::Demo.F2), new global::Demo.F1Attribute(), () => new global::Demo.F2());
        yield return (typeof(global::Demo.F3), new global::Demo.F1Attribute(), () => new global::Demo.F3());
    }
}
```

如果不限制特性，只需导出继承自 `Base` 的所有类型，可省略元组中的特性项：

```csharp
[TelescopeExport]
private static partial IEnumerable<(Type type, Func<Base> creator)> ExportAllSubTypes();
```

#### 扫描引用程序集

在 `TelescopeExportAttribute` 上设置 `IncludeReferences = true` 可一并导出所有引用程序集中满足条件的类型：

```csharp
internal partial class Program
{
    [TelescopeExport(IncludeReferences = true)]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

仅推荐在入口程序集启用 `IncludeReferences = true`，因为任何引用程序集的变更都会触发源生成器重新执行。

### 2. 程序集特性式（`MarkExport` + `AttributedTypes`）

在需要导出类型的项目中添加程序集级 `[assembly: MarkExport(...)]` 特性：

```csharp
using DotNetCampus.Telescope;

[assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
```

源生成器会在编译期自动生成 `DotNetCampus.Telescope.__AttributedTypesExport__`，运行时可通过 `AttributedTypes.FromAssembly` 跨程序集读取：

```csharp
using DotNetCampus.Telescope;

var exportedTypes = AttributedTypes.FromAssembly<Base, FooAttribute>(typeof(Program).Assembly);
foreach (var metadata in exportedTypes)
{
    Console.WriteLine(metadata.RealType.FullName);
    var instance = metadata.CreateInstance();
}
```

## 为此项目开发

非常期望你能加入到 Telescope 的开发中来，请阅读 [如何为 Telescope 贡献代码](docs/zh-hans/how-to-contribute.md) 了解开发相关的约定和技术要求。
