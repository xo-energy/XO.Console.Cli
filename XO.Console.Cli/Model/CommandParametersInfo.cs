using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

internal sealed record CommandParametersInfo(
    ImmutableList<CommandArgument> Arguments,
    ImmutableList<CommandOption> Options);
