using Microsoft.CodeAnalysis;

namespace XO.Console.Cli;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor CommandTypeMustImplementICommand
        = new DiagnosticDescriptor(
            "XOCLI101",
            "Command implementations must implement ICommand<TParameters>",
            "Command class '{0}' must derive from AsyncCommand<TParameters> or Command<TParameters>",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandTypeMustNotBeAbstract
        = new DiagnosticDescriptor(
            "XOCLI102",
            "Command implementations must not be abstract",
            "Command class '{0}' must not be abstract",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandMayNotHaveMultipleCommandAttributes
        = new DiagnosticDescriptor(
            "XOCLI111",
            "Commands may not have multiple command attributes",
            "More than one CommandAttribute (or subclass of CommandAttribute) is applied to command class '{0}'",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateVerbWillBeIgnored
        = new DiagnosticDescriptor(
            "XOCLI121",
            "Duplicate verb will be ignored",
            "Command '{0}' has the same verb '{1}' as '{2}'",
            "XOCLI",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandBranchAttributeMustBeAppliedToCommandAttribute
        = new DiagnosticDescriptor(
            "XOCLI201",
            "CommandBranchAttribute must be applied to a custom attribute that derives from CommandAttribute",
            "CommandBranchAttribute target '{0}' does not derive from CommandAttribute",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandAttributeMustHavePublicConstructor
        = new DiagnosticDescriptor(
            "XOCLI211",
            "Custom CommandAttributes must have a public constructor",
            "Custom CommandAttribute '{0}' must have a public constructor",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandAttributeConstructorsMustHaveVerbParameter
        = new DiagnosticDescriptor(
            "XOCLI212",
            "Custom CommandAttribute constructors must have a first parameter 'string verb'",
            "Custom CommandAttribute constructor '{0}' must have a first parameter 'string verb'",
            "XOCLI",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicatePathWillBeIgnored
        = new DiagnosticDescriptor(
            "XOCLI221",
            "Duplicate path will be ignored",
            "CommandBranchAttribute target '{0}' has the same path '{1}' as '{2}'",
            "XOCLI",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
}
