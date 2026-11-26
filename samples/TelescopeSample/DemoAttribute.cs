namespace DotNetCampus.Telescope.Sample;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DemoAttribute(string? value = null) : Attribute
{
    public string? Value { get; } = value;
}
