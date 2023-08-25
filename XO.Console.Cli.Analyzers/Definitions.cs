using System.Text;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli;

internal static class Definitions
{
    private static readonly SymbolDisplayFormat DisplayFormatForComparison
        = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static readonly SymbolDisplayFormat DisplayFormatForOutput
        = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    public static bool EqualsSourceName(this ITypeSymbol typeSymbol, string sourceName)
        => typeSymbol.ToDisplayString(DisplayFormatForComparison) == sourceName;

    public static string ToSourceString(this ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(DisplayFormatForOutput);
}
