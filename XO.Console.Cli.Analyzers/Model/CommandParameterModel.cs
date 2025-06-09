using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

public abstract record CommandParameterModel
{
    [SetsRequiredMembers]
    protected CommandParameterModel(string name, IPropertySymbol property, string? description)
    {
        this.Name = name;
        this.Description = description;

        var parameterValueType = GetParameterValueType(property.Type);

        this.DeclaringType = property.ContainingType.ToSourceString();
        this.ParameterParsingStrategy = GetParameterParsingStrategy(parameterValueType);
        this.ParameterValueSpecialType = parameterValueType.SpecialType;
        this.ParameterValueType = parameterValueType.ToSourceString();
        this.PropertyName = property.Name;
        this.PropertyTypeKind = property.Type.TypeKind;
    }

    public string Name { get; }
    public string? Description { get; }

    public string DeclaringType { get; }
    public ParameterParsingStrategy ParameterParsingStrategy { get; }
    public SpecialType ParameterValueSpecialType { get; }
    public string ParameterValueType { get; }
    public string PropertyName { get; }
    public TypeKind PropertyTypeKind { get; }

    public static ITypeSymbol GetParameterValueType(ITypeSymbol propertyType)
    {
        if (propertyType.TypeKind == TypeKind.Array)
            return ((IArrayTypeSymbol)propertyType).ElementType;

        // Check for Nullable<T> and return the underlying type
        if (propertyType is INamedTypeSymbol { IsGenericType: true } namedType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T &&
            namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0];
        }

        return propertyType;
    }

    public static ParameterParsingStrategy GetParameterParsingStrategy(ITypeSymbol parameterValueType)
    {
        switch (parameterValueType.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_DateTime:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_SByte:
            case SpecialType.System_Single:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
                return ParameterParsingStrategy.Parse;

            case SpecialType.System_Char:
                return ParameterParsingStrategy.Char;

            case SpecialType.System_String:
                return ParameterParsingStrategy.String;

            case SpecialType.None when parameterValueType.TypeKind == TypeKind.Enum:
                return ParameterParsingStrategy.Enum;

            case SpecialType.None when parameterValueType is INamedTypeSymbol namedType:
                return GetParameterParsingStrategyForNamedType(namedType);

            default:
                return ParameterParsingStrategy.None;
        }
    }

    public static ParameterParsingStrategy GetParameterParsingStrategyForNamedType(INamedTypeSymbol parameterValueType)
    {
        // find a public static 'Parse' method with a single string parameter
        foreach (var member in parameterValueType.GetMembers("Parse"))
        {
            if (member.IsStatic && member.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)member;
                if (method.DeclaredAccessibility == Accessibility.Public &&
                    method.Parameters.Length == 1 &&
                    method.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                    method.ReturnType.Equals(parameterValueType, SymbolEqualityComparer.Default))
                {
                    return ParameterParsingStrategy.Parse;
                }
            }
        }

        // find a public constructor with a single string parameter
        foreach (var ctor in parameterValueType.InstanceConstructors)
        {
            if (ctor.DeclaredAccessibility == Accessibility.Public &&
                ctor.Parameters.Length == 1 &&
                ctor.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                return ParameterParsingStrategy.Constructor;
            }
        }

        // give up
        return ParameterParsingStrategy.None;
    }
}
