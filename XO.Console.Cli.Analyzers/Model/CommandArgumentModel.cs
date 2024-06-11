using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

public sealed record CommandArgumentModel : CommandParameterModel, ICommandArgumentAttributeData
{
    [SetsRequiredMembers]
    public CommandArgumentModel(string name, IPropertySymbol property, string? description)
        : base(name, property, description) { }

    public int Order { get; init; }

    public bool IsGreedy { get; init; }

    public bool IsOptional { get; init; }
}
