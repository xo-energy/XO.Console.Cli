using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

internal enum CommandModelKind
{
    Branch,
    Command,
}

internal sealed record CommandModel(
    CommandModelKind Kind,
    string FullName,
    LocationInfo? LocationInfo,
    ImmutableList<DiagnosticInfo> Diagnostics,
    ImmutableArray<string> Path = default,
    ImmutableArray<string> Aliases = default,
    string? Description = null,
    bool IsHidden = false,
    string? ParametersType = null)
{
    public static CommandModel FromAttributeData(
        CommandModelKind kind,
        string fullName,
        LocationInfo? location,
        ImmutableList<DiagnosticInfo> diagnostics,
        CommandAttributeData? attributeData,
        string? parametersType = null)
    {
        return new CommandModel(
            kind,
            fullName,
            location,
            diagnostics,
            attributeData?.Path ?? default,
            attributeData?.Aliases ?? default,
            attributeData?.Description,
            attributeData?.IsHidden ?? default,
            parametersType ?? attributeData?.ParametersType);
    }

    public bool Equals(CommandModel? other)
    {
        if ((object?)this == (object?)other)
            return true;

        return other != null
            && this.Kind == other.Kind
            && this.FullName == other.FullName
            && this.LocationInfo == other.LocationInfo
            && Enumerable.SequenceEqual(this.Diagnostics, other.Diagnostics)
            && ImmutableArrayEqualityComparer.Equals(this.Path, other.Path)
            && ImmutableArrayEqualityComparer.Equals(this.Aliases, other.Aliases)
            && this.Description == other.Description
            && this.IsHidden == other.IsHidden
            && this.ParametersType == other.ParametersType;
    }

    public override int GetHashCode()
    {
        // optimize for speed; we're not using this type as a key and would rather get to the Equals() call faster
        return HashCode.Combine(this.Kind, this.FullName);
    }

    public bool HasBuilderOptions
        => Aliases.IsDefaultOrEmpty == false
        || Description is not null
        || IsHidden;

    public bool HasDeclarativeConfiguration
        => Path.IsDefaultOrEmpty == false;

    public string? Verb
        => HasDeclarativeConfiguration ? Path[^1] : null;
}
