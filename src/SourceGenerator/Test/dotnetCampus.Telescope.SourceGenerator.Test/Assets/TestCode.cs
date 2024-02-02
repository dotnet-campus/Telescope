using dotnetCampus.Telescope.SourceGeneratorAnalyzers.TestLib1;

using System;

namespace dotnetCampus.Telescope.SourceGenerator.Test.Assets;

[F1]
public class CurrentFoo : dotnetCampus.Telescope.SourceGeneratorAnalyzers.TestLib1.F1
{
}

[Foo(0, FooEnum.N1, typeof(Foo), null)]
abstract class F1 : Base
{
}

[FooAttribute()]
class Foo : Base
{
}

class Base
{
}

public enum FooEnum
{
    N1,
    N2,
    N3,
}

class FooAttribute : Attribute
{
    public FooAttribute()
    {
    }

    public FooAttribute(ulong number1, FooEnum fooEnum, Type? type1, Type? type3)
    {
        Number1 = number1;
        FooEnum1 = fooEnum;
        Type1 = type1;
    }

    public ulong Number1 { get; set; }
    public long Number2 { get; set; }

    public FooEnum FooEnum1 { get; set; }
    public FooEnum FooEnum2 { get; set; }
    public FooEnum FooEnum3 { get; set; }

    public Type? Type1 { get; set; }
    public Type? Type2 { get; set; }
    public Type? Type3 { get; set; }
}