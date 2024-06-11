using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace XO.Console.Cli.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandAttributeTargetAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        DiagnosticDescriptors.CommandTypeMustImplementICommand,
        DiagnosticDescriptors.CommandTypeMustNotBeAbstract,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        // only consider class declarations (compiler will enforce AttributeUsage)
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } namedType)
            return;

        if (HasCommandAttribute(namedType))
        {
            if (!ImplementsICommand(namedType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandTypeMustImplementICommand,
                        namedType.Locations.FirstOrDefault(),
                        namedType.ToSourceString()));
            }
            else if (namedType.IsAbstract)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandTypeMustNotBeAbstract,
                        namedType.Locations.FirstOrDefault(),
                        namedType.ToSourceString()));
            }
        }
    }

    private static bool HasCommandAttribute(INamedTypeSymbol namedType)
    {
        foreach (var attribute in namedType.GetAttributes())
        {
            INamedTypeSymbol? attributeType;
            for (
                attributeType = attribute.AttributeClass;
                attributeType != null;
                attributeType = attributeType.BaseType)
            {
                if (attributeType.EqualsSourceString("XO.Console.Cli.CommandAttribute"))
                    return true;
            }
        }

        return false;
    }

    private static bool ImplementsICommand(INamedTypeSymbol namedType)
    {
        foreach (var @interface in namedType.AllInterfaces)
        {
            if (@interface.ConstructedFrom.EqualsSourceString("XO.Console.Cli.ICommand<TParameters>"))
                return true;
        }

        return false;
    }
}
