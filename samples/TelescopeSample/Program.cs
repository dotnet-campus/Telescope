namespace DotNetCampus.Telescope.Sample;

internal static partial class Program
{
    private static void Main()
    {
        Console.WriteLine("=== 1. [TelescopeExport] 分部方法导出 ===");
        foreach (var (type, attribute, creator) in ExportDemos())
        {
            var instance = creator();
            Console.WriteLine($"Type = {type.FullName}, Attribute = {attribute.Value}, Instance = {instance.GetHashCode()}");
        }

        Console.WriteLine("=== 2. [assembly: MarkExport] + AttributedTypes.FromAssembly 导出 ===");
        var attributedTypes = AttributedTypes.FromAssembly<IDemo, DemoAttribute>(typeof(Program).Assembly);
        foreach (var attributedType in attributedTypes)
        {
            var type = attributedType.RealType;
            var attribute = attributedType.Attribute;
            var instance = attributedType.CreateInstance();
            Console.WriteLine($"Type = {type.FullName}, Attribute = {attribute.Value}, Instance = {instance.GetHashCode()}");
        }
    }

    [TelescopeExport]
    private static partial IEnumerable<(Type type, DemoAttribute attribute, Func<IDemo> creator)> ExportDemos();
}
