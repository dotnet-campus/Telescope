using DotNetCampus.Telescope.Tests.Lib1;

using System;

namespace DotNetCampus.Telescope.Tests.Assets;

[F1]
public class CurrentFoo : DotNetCampus.Telescope.Tests.Lib1.F1
{
}

[Foo(0, FooEnum.N1, typeof(Foo), null)]
abstract class F1 : Base
{
}

[FooAttribute()]
public class Foo : Base
{
}

[Foo(1, FooEnum.N2, typeof(Base), null, Number2 = 42, Text = "hello \"world\"\nline2", Tags = new[] { "a", "b" })]
public class FooWithComplexAttribute : Base
{
}

public class Base
{
}

public enum FooEnum
{
    N1,
    N2,
    N3,
}

public class FooAttribute : Attribute
{
    public FooAttribute()
    {
    }

    public FooAttribute(ulong number1, FooEnum fooEnum, Type? type1, Type? type3)
    {
        Number1 = number1;
        FooEnum1 = fooEnum;
        Type1 = type1;
        Type3 = type3;
    }

    public ulong Number1 { get; set; }
    public long Number2 { get; set; }

    public FooEnum FooEnum1 { get; set; }
    public FooEnum FooEnum2 { get; set; }
    public FooEnum FooEnum3 { get; set; }

    public Type? Type1 { get; set; }
    public Type? Type2 { get; set; }
    public Type? Type3 { get; set; }

    public string? Text { get; set; }
    public string[]? Tags { get; set; }
}
