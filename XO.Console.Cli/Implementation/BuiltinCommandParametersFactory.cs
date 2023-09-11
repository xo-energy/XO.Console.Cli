using System.Collections.Immutable;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Implementation;

internal sealed class BuiltinCommandParametersFactory : ICommandParametersFactory
{
    public static readonly ICommandParametersFactory Instance
        = new BuiltinCommandParametersFactory();

    public CommandParametersInfo? DescribeParameters(Type parametersType)
    {
        if (parametersType == typeof(CommandParameters))
        {
            return new CommandParametersInfo(
                ImmutableArray<CommandArgument>.Empty,
                ImmutableArray<CommandOption>.Empty);
        }

        return null;
    }
}
