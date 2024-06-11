using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

internal readonly record struct DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? LocationInfo,
    string? arg0 = null)
{
    public Diagnostic CreateDiagnostic()
        => Diagnostic.Create(Descriptor, LocationInfo?.ToLocation(), arg0);
}
