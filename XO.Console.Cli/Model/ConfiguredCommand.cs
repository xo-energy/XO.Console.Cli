using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Model;

internal sealed record ConfiguredCommand(
    CommandFactory CommandFactory,
    [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type ParametersType,
    string Verb)
{
    public ImmutableHashSet<string> Aliases { get; init; }
        = ImmutableHashSet<string>.Empty;

    public IImmutableList<ConfiguredCommand> Commands { get; init; }
        = ImmutableList<ConfiguredCommand>.Empty;

    public string? Description { get; init; }

    public bool IsHidden { get; init; }

    public bool IsMatch(string verb)
    {
        return this.Verb == verb
            || this.Aliases.Contains(verb);
    }
}
