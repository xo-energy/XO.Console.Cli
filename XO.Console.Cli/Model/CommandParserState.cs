using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using XO.Console.Cli.Infrastructure;

namespace XO.Console.Cli.Model;

internal sealed class CommandParserState
{
    private readonly CommandAppSettings _settings;
    private readonly Regex _optionValidationPattern;

    public CommandParserState(int count, ImmutableList<ConfiguredCommand> commands, CommandAppSettings settings)
    {
        this.Commands = commands;
        this.Arguments = new();
        this.Options = new(settings.OptionNameComparer);
        this.ParametersSeen = new();
        this.Tokens = new CommandToken[count];
        this.Errors = ImmutableList.CreateBuilder<string>();

        _settings = settings;
        _optionValidationPattern = settings.OptionStyle.GetNameValidationPattern();
    }

    public ImmutableList<ConfiguredCommand> Commands { get; set; }
    public Queue<CommandArgument> Arguments { get; }
    public Dictionary<string, CommandOption> Options { get; }
    public HashSet<AbstractCommandParameter> ParametersSeen { get; }
    public CommandToken[] Tokens { get; }
    public ImmutableList<string>.Builder Errors { get; }
    public bool ExplicitArguments { get; set; }

    public void AddOption(Type parametersType, CommandOption option)
    {
        if (!this.ParametersSeen.Add(option))
            return;

        ValidateOptionName(parametersType, option.Name);
        this.Options.Add(option.Name, option);

        foreach (var alias in option.Aliases)
        {
            ValidateOptionName(parametersType, alias);
            this.Options.Add(alias, option);
        }
    }

    public void AddParameters(ConfiguredCommand configuredCommand)
    {
        Debug.Assert(this.Arguments.Count == 0);

        var parametersInfo = TypeRegistry.DescribeParameters(configuredCommand.ParametersType);

        CommandArgument? previous = null;
        foreach (var argument in parametersInfo.Arguments)
        {
            if (!this.ParametersSeen.Add(argument))
                continue;

            ValidateArgument(configuredCommand, argument, previous);
            this.Arguments.Enqueue(argument);

            previous = argument;
        }

        foreach (var option in parametersInfo.Options)
            AddOption(configuredCommand.ParametersType, option);
    }

    public bool TryGetOption(string[] parts, [NotNullWhen(true)] out CommandOption? option, out string? value)
    {
        // look up the option (support '--option[=:]value' style)
        if (this.Options.TryGetValue(parts[0], out option))
        {
            value = parts.Length > 1 ? parts[1] : null;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    public bool TryGetShortOptionGroup(string arg, out ImmutableList<CommandOption> optionGroup)
    {
        optionGroup = ImmutableList<CommandOption>.Empty;

        // not allowed for option styles that don't differentiate between short and long options
        if (!_settings.OptionStyle.HasShortOptions())
            return false;

        for (int i = 1; i < arg.Length; ++i)
        {
            // every letter in the group must be a valid short option
            if (!this.Options.TryGetValue($"{_settings.OptionLeader}{arg[i]}", out var option))
                return false;

            // every option in the group must be a flag
            if (!option.IsFlag)
                return false;

            optionGroup = optionGroup.Add(option);
        }

        return !optionGroup.IsEmpty;
    }

    private void ValidateArgument(ConfiguredCommand configuredCommand, CommandArgument argument, CommandArgument? previous)
    {
        if (previous?.IsGreedy == true)
        {
            throw new CommandTypeException(
                configuredCommand.ParametersType,
                $"Argument '{previous}' is greedy, but there are other arguments after it");
        }

        if (previous?.IsOptional == true && !argument.IsOptional)
        {
            throw new CommandTypeException(
                configuredCommand.ParametersType,
                $"Argument '{argument}' is required, but the previous argument was optional");
        }

        if (argument.IsGreedy && configuredCommand.Commands.Count > 0)
        {
            throw new CommandTypeException(
                configuredCommand.ParametersType,
                $"Command '{configuredCommand.Verb}' has subcommands, but its argument '{argument.Name}' is greedy");
        }

        if (argument.IsOptional && configuredCommand.Commands.Count > 0)
        {
            throw new CommandTypeException(
                configuredCommand.ParametersType,
                $"Command '{configuredCommand.Verb}' has subcommands, but its argument '{argument.Name}' is optional");
        }
    }

    private void ValidateOptionName(Type parametersType, string name)
    {
        if (!_optionValidationPattern.IsMatch(name))
            throw new CommandTypeException(parametersType, $"Option name '{name}' is invalid");
        if (this.Options.ContainsKey(name))
            throw new CommandTypeException(parametersType, $"Duplicate option name '{name}'");
    }
}
