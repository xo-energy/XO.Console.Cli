using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli;

internal static class Definitions
{
    private static readonly SymbolDisplayFormat DisplayFormat
        = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    public static ImmutableArray<TValue> ConvertTypedConstantToImmutableArray<TValue>(TypedConstant constant)
    {
        var builder = ImmutableArray.CreateBuilder<TValue>(constant.Values.Length);

        foreach (var constantValue in constant.Values)
            builder.Add((TValue)constantValue.Value!);

        return builder.MoveToImmutable();
    }

    public static bool EqualsSourceString(this ITypeSymbol typeSymbol, string sourceName)
        => typeSymbol.ToDisplayString(DisplayFormat) == sourceName;

    public static string ToSourceString(this ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(DisplayFormat);
}
