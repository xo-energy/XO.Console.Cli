using System.Collections.Immutable;

namespace XO.Console.Cli;

internal sealed record CommandParametersInfo(
    ImmutableList<CommandArgument> Arguments,
    ImmutableList<CommandOption> Options);
