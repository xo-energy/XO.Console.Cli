using System.Collections.Immutable;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Commands;

internal sealed class CliExplainCommand : Command
{
    public override int Execute(ICommandContext context, CancellationToken cancellationToken)
    {
        var indent = "";

        foreach (var token in context.ParseResult.Tokens)
        {
            string? description = token.Context switch
            {
                AbstractCommandParameter parameter when
                    token.TokenType is CommandTokenType.OptionValue or CommandTokenType.Argument
                    => $"{parameter.ValueType} '{parameter.Name}'",
                AbstractCommandParameter parameter => parameter.ToString(),
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

        if (context.ParseResult.Errors.Any())
        {
            context.Console.Output.WriteLine();

            foreach (var error in context.ParseResult.Errors)
                context.Console.Output.WriteLine($"Error: {error}");
        }

        context.Console.Output.WriteLine();
        return 0;
    }
}
