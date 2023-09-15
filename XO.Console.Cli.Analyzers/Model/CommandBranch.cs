using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

internal sealed record CommandBranch(
    ImmutableArray<string> Path,
    ImmutableArray<string> Branch)
{
    public bool Equals(CommandBranch? other)
        => other != null
        && ImmutableArrayEqualityComparer.Equals(this.Path, other.Path)
        && ImmutableArrayEqualityComparer.Equals(this.Branch, other.Branch);

    public override int GetHashCode()
    {
        var hash = HashCode.Initialize();

        hash = HashCode.Add(hash, ImmutableArrayEqualityComparer.GetHashCode(this.Path));
        hash = HashCode.Add(hash, ImmutableArrayEqualityComparer.GetHashCode(this.Branch));

        return hash;
    }
}
