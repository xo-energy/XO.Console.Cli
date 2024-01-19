using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

/// <summary>
/// Describes the parameters defined by a command parameters type.
/// </summary>
/// <param name="Arguments">The collection of arguments.</param>
/// <param name="Options">The collection of options.</param>
public sealed record CommandParametersInfo(
    ImmutableArray<CommandArgument> Arguments,
    ImmutableArray<CommandOption> Options);
