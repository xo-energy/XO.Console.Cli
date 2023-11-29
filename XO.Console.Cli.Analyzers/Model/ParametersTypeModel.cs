using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

public sealed record ParametersTypeModel(
    string Name,
    ImmutableArray<CommandArgumentModel> Arguments,
    ImmutableArray<CommandOptionModel> Options)
{
    public bool Equals(ParametersTypeModel? other)
    {
        if ((object?)this == (object?)other)
            return true;

        return other != null
            && this.Name == other.Name
            && ImmutableArrayEqualityComparer.Equals(this.Arguments, other.Arguments)
            && ImmutableArrayEqualityComparer.Equals(this.Options, other.Options)
            ;
    }

    public override int GetHashCode()
    {
        // optimize for speed; we're not using this type as a key and would rather get to the Equals() call faster
        return Name.GetHashCode();
    }
}
