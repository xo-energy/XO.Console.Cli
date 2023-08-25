using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

internal sealed record CommandOptionModel : CommandParameterModel, ICommandOptionAttributeData
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
}
