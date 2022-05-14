using System.Collections.Immutable;

namespace XO.Console.Cli.Commands;

internal sealed class CliExplainCommand : Command
{
    private readonly CommandParseResult _parse;

    public CliExplainCommand(CommandParseResult parse)
    {
        _parse = parse;
    }

    public override int Execute(ICommandContext context, CancellationToken cancellationToken)
    {
        var indent = "";

        foreach (var token in _parse.Tokens)
        {
            string? description = token.Context switch
            {
                CommandParameter parameter when
                    token.TokenType is CommandTokenType.OptionValue or CommandTokenType.Argument
                    => $"{parameter.ValueType} '{parameter.Name}'",
                CommandParameter parameter => parameter.ToString(),
                ConfiguredCommand command => command.ParametersType.ToString(),
                IImmutableList<CommandOption> group when
                    token.TokenType is CommandTokenType.OptionGroup
                    => String.Join(", ", from option in @group select option.Name),
                _ => token.Context?.ToString()
            };

            context.Console.Output.WriteLine($"{indent}{token.TokenType,-11}  {token.Value} ({description})");

            if (token.TokenType == CommandTokenType.Command)
                indent += "   ";
        }

        if (_parse.Errors.Any())
        {
            context.Console.Output.WriteLine();

            foreach (var error in _parse.Errors)
                context.Console.Output.WriteLine($"Error: {error}");
        }

        context.Console.Output.WriteLine();
        return 0;
    }
}
