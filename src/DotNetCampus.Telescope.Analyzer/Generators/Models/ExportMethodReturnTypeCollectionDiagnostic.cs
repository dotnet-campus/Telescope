using Microsoft.CodeAnalysis;

namespace DotNetCampus.Telescope.Generators.Models;

internal class ExportMethodReturnTypeCollectionDiagnostic(
    DiagnosticDescriptor diagnosticDescriptor,
    Location? location = null,
    params object[]? messageArgs) : IExportMethodReturnTypeCollectionResult
{
    public Location? Location { get; set; } = location;

    public DiagnosticDescriptor DiagnosticDescriptor { get; } = diagnosticDescriptor;

    public object[]? MessageArgs { get; set; } = messageArgs;

    public Diagnostic ToDiagnostic() => Diagnostic.Create(DiagnosticDescriptor, Location, MessageArgs);
}
