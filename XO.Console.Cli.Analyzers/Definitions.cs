using Microsoft.CodeAnalysis;

namespace XO.Console.Cli;

internal static class Definitions
{
    private static readonly SymbolDisplayFormat DisplayFormat
        = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    public static bool EqualsSourceString(this ITypeSymbol typeSymbol, string sourceName)
        => typeSymbol.ToDisplayString(DisplayFormat) == sourceName;

    public static string ToSourceString(this ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(DisplayFormat);
}
