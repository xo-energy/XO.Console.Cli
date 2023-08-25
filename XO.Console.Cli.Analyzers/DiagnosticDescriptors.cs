using Microsoft.CodeAnalysis;

namespace XO.Console.Cli;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor CommandTypeMustImplementICommand
        = new DiagnosticDescriptor(
            "XOCLI1001",
            "Command implementations must implement ICommand<TParameters>",
            "Command class '{0}' must derive from AsyncCommand<TParameters> or Command<TParameters>",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}
