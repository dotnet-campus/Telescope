# Telescope

Telescope is a pre-compilation framework powered by Roslyn Incremental Source Generators.

|[中文](../../README.md)|[English](README.md)|
|-|-|

Once `DotNetCampus.Telescope` is installed in a project, types and attribute metadata are discovered and emitted at compile time, eliminating expensive runtime reflection and significantly improving startup and runtime performance.

[![.NET Build](https://github.com/dotnet-campus/Telescope/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/dotnet-campus/Telescope/actions/workflows/dotnet-build.yml)

|Package|NuGet|
|--|--|
|DotNetCampus.Telescope|[![NuGet](https://img.shields.io/nuget/v/DotNetCampus.Telescope.svg)](https://www.nuget.org/packages/DotNetCampus.Telescope)|

Supported target frameworks: `net6.0`, `netstandard2.0`, `net45`.

## Installation

```xml
<PackageReference Include="DotNetCampus.Telescope" Version="1.0.0" />
```

`DotNetCampus.Telescope` ships both the runtime API and the Roslyn incremental source generators in a single NuGet package.

## Usage

### 1. Partial Method Style (Recommended)

Declare a partial method in a partial class, mark it with `[TelescopeExport]`, and specify the export criteria in its return type:

```csharp
using DotNetCampus.Telescope;

internal partial class Program
{
    [TelescopeExport]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

The source generator emits the implementation returning all non-abstract types deriving from `Base` and marked with `F1Attribute`:

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

To export all derived types without requiring an attribute, omit the attribute element from the tuple:

```csharp
[TelescopeExport]
private static partial IEnumerable<(Type type, Func<Base> creator)> ExportAllSubTypes();
```

#### Including Referenced Assemblies

Set `IncludeReferences = true` on `TelescopeExportAttribute` to also scan referenced assemblies:

```csharp
internal partial class Program
{
    [TelescopeExport(IncludeReferences = true)]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

It is recommended to enable `IncludeReferences = true` only in the entry assembly, because changes in referenced assemblies will re-trigger source generation.

### 2. Assembly Attribute Style (`MarkExport` + `AttributedTypes`)

Mark the assembly with `[assembly: MarkExport(...)]`:

```csharp
using DotNetCampus.Telescope;

[assembly: MarkExport(typeof(Base), typeof(FooAttribute))]
```

The source generator generates `DotNetCampus.Telescope.__AttributedTypesExport__`, which can be queried across assemblies at runtime via `AttributedTypes.FromAssembly`:

```csharp
using DotNetCampus.Telescope;

var exportedTypes = AttributedTypes.FromAssembly<Base, FooAttribute>(typeof(Program).Assembly);
foreach (var metadata in exportedTypes)
{
    Console.WriteLine(metadata.RealType.FullName);
    var instance = metadata.CreateInstance();
}
```
