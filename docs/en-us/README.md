# Telescope

Telescope is a pre-compilation framework.

Once Telescope is installed in a project, you can start writing pre-compilation code in the project. By executing these pre-compilation codes, the project can execute some codes during compilation that usually need to be executed at runtime. This method can migrate time-consuming runtime code to compile-time execution, greatly improving runtime performance.

## Telescope.SourceGeneratorAnalyzers

The version of Telescope using SourceGenerator source code generator

It can be used to export specified types

### Usage

Supports multiple different export methods

#### Partial Method Style

This is the recommended method

Define partial methods in the partial class, mark the partial method with the `dotnetCampus.Telescope.TelescopeExportAttribute` attribute, and the return value includes export conditions, as follows

```csharp
internal partial class Program
{
    [dotnetCampus.Telescope.TelescopeExportAttribute()]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

The above code will export all types in the current project that are marked with `F1Attribute` and inherit `Base`. After the Telescope source code generator, the code generated is roughly as follows

```csharp
    [global::System.CodeDom.Compiler.GeneratedCode("dotnetCampus.Telescope.SourceGeneratorAnalyzers", "1.0.0")]
    internal partial class Program
    {
        private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable()
        {
            yield return (typeof(F2), new F1Attribute()
            {
                       
            }, () => new F2());
            yield return (typeof(F3), new F1Attribute()
            {
                       
            }, () => new F3());
        }
    }
```

Advanced usage:

You can add the IncludeReference attribute to TelescopeExportAttribute to export all types of referenced assemblies that meet the conditions, as follows

```csharp
internal partial class Program
{
    [dotnetCampus.Telescope.TelescopeExportAttribute(IncludeReference = true)]
    private static partial IEnumerable<(Type type, F1Attribute attribute, Func<Base> creator)> ExportFooEnumerable();
}
```

It is only recommended to add the `IncludeReference = true` attribute in the entry assembly, because once this attribute is added, any changes to the referenced assembly may cause the source code generator to execute repeatedly, reducing VisualStudio performance

#### Assembly Marking

This is the traditional implementation method of Telescope. In the project where you need to export types, mark the `dotnetCampus.Telescope.MarkExportAttribute` attribute, as follows

```csharp
[assembly: dotnetCampus.Telescope.MarkExportAttribute(typeof(Base), typeof(FooAttribute))]
```

After marking, the `dotnetCampus.Telescope.__AttributedTypesExport__` type will be automatically generated, and you can directly use it in the code, as follows

```csharp
        var attributedTypesExport = new __AttributedTypesExport__();
        ICompileTimeAttributedTypesExporter<Base, FooAttribute> exporter = attributedTypesExport;
        foreach (var exportedTypeMetadata in exporter.ExportAttributeTypes())
        {
            Console.WriteLine(exportedTypeMetadata.RealType.FullName);
        }
```

You can also use the `dotnetCampus.Telescope.AttributedTypes` helper class to get all exported types

## Develop for this project

We very much hope that you can join the development of Telescope, please read [How to Contribute to Telescope](/docs/how-to-contribute.md) to understand the development related conventions and technical requirements.
