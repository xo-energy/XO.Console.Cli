using System.Collections.Immutable;

namespace XO.Console.Cli.Commands;

internal sealed class HelpCommand : AsyncCommand
{
    private readonly CommandApp _app;
    private readonly CommandParametersInspector _inspector;

    public HelpCommand(CommandApp app, CommandParametersInspector inspector)
    {
        _app = app;
        _inspector = inspector;
    }

    public override async Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
    {
        var command = _app.RootCommand;
        var commands = GetVisibleCommands(command);
        var parametersInfo = _inspector.InspectParameters(command);
        var arguments = new List<CommandArgument>(0);
        var argumentsSeen = new HashSet<CommandArgument>();
        var usageParts = new List<string>() { _app.Settings.ApplicationName };

        foreach (var token in context.ParseResult.Tokens)
        {
            if (token.TokenType != CommandTokenType.Command)
                continue;

            command = (ConfiguredCommand)token.Context!;
            commands = GetVisibleCommands(command);
            parametersInfo = _inspector.InspectParameters(command);

            usageParts.Add(command.Verb);

            foreach (var argument in parametersInfo.Arguments)
            {
                if (!argumentsSeen.Add(argument))
                    continue;

                var (bracketL, bracketR) = argument.Attribute.IsOptional
                    ? ('[', ']')
                    : ('<', '>');

                usageParts.Add(
                    $"{bracketL}{argument.Attribute.Name}{(argument.Attribute.IsGreedy ? " ... " : "")}{bracketR}");

                arguments.Add(argument);
            }
        }

        var options = GetVisibleOptions(parametersInfo);
        if (options.Any() || _app.Settings.GlobalOptions.Any())
            usageParts.Add("[OPTIONS]");

        if (commands.Any())
            usageParts.Add("COMMAND");

        await context.Console.Output.WriteLineAsync(
            $"{_app.Settings.ApplicationName} {_app.Settings.ApplicationVersion}")
            .ConfigureAwait(false);

        if (command.Description != null)
        {
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync(command.Description).ConfigureAwait(false);
        }

        await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
        await context.Console.Output.WriteLineAsync("USAGE").ConfigureAwait(false);
        await context.Console.Output.WriteLineAsync($"  {String.Join(' ', usageParts)}").ConfigureAwait(false);

        if (arguments.Any())
        {
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync("ARGUMENTS").ConfigureAwait(false);

            var length = arguments.Max(x => x.Name.Length);

            foreach (var argument in arguments)
            {
                await context.Console.Output.WriteLineAsync(
                    $"  {argument.Name.PadRight(length)}    {argument.Description}")
                    .ConfigureAwait(false);
            }
        }

        if (commands.Any())
        {
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync("COMMANDS").ConfigureAwait(false);

            var length = commands.Max(x => x.Verb.Length);

            foreach (var x in commands.OrderBy(x => x.Verb))
            {
                await context.Console.Output.WriteLineAsync(
                    $"  {x.Verb.PadRight(length)}    {x.Description}")
                    .ConfigureAwait(false);
            }
        }

        var optionsLength = GetBuiltinOptions(command).Max(x => x.Name.Length);
        foreach (var option in Enumerable.Concat(options, _app.Settings.GlobalOptions))
        {
            var length = GetMaxLength(option.GetNames());
            if (optionsLength < length)
                optionsLength = length;
        }

        await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
        await context.Console.Output.WriteLineAsync("OPTIONS").ConfigureAwait(false);

        if (options.Any())
        {
            await WriteOptionsAsync(context, optionsLength, options).ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
        }

        if (_app.Settings.GlobalOptions.Any())
        {
            await WriteOptionsAsync(context, optionsLength, _app.Settings.GlobalOptions).ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
        }

        await WriteOptionsAsync(context, optionsLength, GetBuiltinOptions(command)).ConfigureAwait(false);

        if (command.Aliases.Any())
        {
            await context.Console.Output.WriteLineAsync().ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync("ALIASES").ConfigureAwait(false);
            await context.Console.Output.WriteLineAsync($"  {String.Join(", ", command.Aliases)}").ConfigureAwait(false);
        }

        return 0;
    }

    private IEnumerable<CommandOption> GetBuiltinOptions(ConfiguredCommand command)
    {
        foreach (var option in _app.BuiltinOptions)
        {
            if (option.Attribute.IsHidden)
                continue;

            if (option == _app.BuiltinOptions.Version && command != _app.RootCommand)
                continue;

            yield return option;
        }
    }

    private ImmutableList<ConfiguredCommand> GetVisibleCommands(ConfiguredCommand command)
    {
        var builder = ImmutableList.CreateBuilder<ConfiguredCommand>();

        foreach (var subcommand in command.Commands)
        {
            if (subcommand.IsHidden)
                continue;

            builder.Add(subcommand);
        }

        return builder.ToImmutable();
    }

    private ImmutableList<CommandOption> GetVisibleOptions(CommandParametersInfo parametersInfo)
    {
        var builder = ImmutableList.CreateBuilder<CommandOption>();

        foreach (var option in parametersInfo.Options)
        {
            if (option.Attribute.IsHidden)
                continue;

            builder.Add(option);
        }

        return builder.ToImmutable();
    }

    private static int GetMaxLength(IEnumerable<string> values)
        => values.Select(x => x.Length).DefaultIfEmpty().Max();

    private async Task WriteOptionsAsync(ICommandContext context, int length, IEnumerable<CommandOption> options)
    {
        foreach (var option in options)
        {
            string? shortName = null;

            foreach (var alias in option.Attribute.Aliases)
            {
                if (alias.Length == 2)
                {
                    shortName = alias;
                    break;
                }
            }

            if (shortName != null)
            {
                await context.Console.Output.WriteLineAsync(
                    $"  {shortName,2}, {option.Name.PadRight(length)}    {option.Description}")
                    .ConfigureAwait(false);
            }
            else
            {
                await context.Console.Output.WriteLineAsync(
                    $"      {option.Name.PadRight(length)}    {option.Description}")
                    .ConfigureAwait(false);
            }

            foreach (var alias in option.Attribute.Aliases)
            {
                if (alias == shortName)
                    continue;

                await context.Console.Output.WriteLineAsync(
                    $"      {alias}")
                    .ConfigureAwait(false);
            }
        }
    }
}
