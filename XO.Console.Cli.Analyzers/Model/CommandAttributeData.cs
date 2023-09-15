using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

internal sealed record CommandAttributeData(
    ImmutableArray<string> Path,
    ImmutableArray<string> Aliases,
    string? Description,
    bool IsHidden,
    string? ParametersType)
{
    public bool Equals(CommandAttributeData? other)
    {
        if ((object)this == (object?)other)
            return true;

        return other != null
            && ImmutableArrayEqualityComparer.Equals(this.Path, other.Path)
            && ImmutableArrayEqualityComparer.Equals(this.Aliases, other.Aliases)
            && this.Description == other.Description
            && this.IsHidden == other.IsHidden
            && this.ParametersType == other.ParametersType;
    }

    public override int GetHashCode()
    {
        // optimize for speed; we're not using this type as a key and would rather get to the Equals() call faster
        return ImmutableArrayEqualityComparer.GetHashCode(this.Path);
    }
}
