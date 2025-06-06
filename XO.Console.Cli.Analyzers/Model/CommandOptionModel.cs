using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

public sealed record CommandOptionModel : CommandParameterModel, ICommandOptionAttributeData
{
    [SetsRequiredMembers]
    public CommandOptionModel(string name, IPropertySymbol property, string? description, ImmutableArray<string> aliases)
        : base(name, property, description)
    {
        this.Aliases = aliases;
        this.IsFlag = base.ParameterValueSpecialType == SpecialType.System_Boolean;
    }

    public ImmutableArray<string> Aliases { get; }
    public bool IsFlag { get; }
    public bool IsHidden { get; init; }
    public bool IsRequired { get; init; }

    public bool Equals(CommandOptionModel? other)
    {
        if ((object?)this == (object?)other)
            return true;

        return other != null
            && base.Equals(other)
            && ImmutableArrayEqualityComparer.Equals(this.Aliases, other.Aliases)
            && this.IsFlag == other.IsFlag
            && this.IsHidden == other.IsHidden
            ;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
