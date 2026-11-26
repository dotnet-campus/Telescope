namespace DotNetCampus.Telescope.Tests.Lib1;

public class F1
{
}

public class F1Attribute : Attribute
{
    public F1Attribute()
    {
    }

    public F1Attribute(string[] tags)
    {
        Tags = tags;
    }

    public string[]? Tags { get; set; }
}
