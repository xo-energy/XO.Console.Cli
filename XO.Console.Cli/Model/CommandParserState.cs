using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using XO.Console.Cli.Infrastructure;

namespace XO.Console.Cli.Model;

internal sealed class CommandParserState
{
    private readonly CommandAppSettings _settings;

    public CommandParserState(int count, IImmutableList<ConfiguredCommand> commands, CommandAppSettings settings)
    {
        this.Commands = commands;
        this.Arguments = new();
        this.Options = new(settings.OptionNameComparer);
        this.ParametersSeen = new();
        this.Tokens = new CommandToken[count];
        this.Errors = ImmutableList.CreateBuilder<string>();

        _settings = settings;
    }

    public IImmutableList<ConfiguredCommand> Commands { get; set; }
    public Queue<CommandArgument> Arguments { get; }
    public Dictionary<string, CommandOption> Options { get; }
    public HashSet<CommandParameter> ParametersSeen { get; }
    public CommandToken[] Tokens { get; }
    public ImmutableList<string>.Builder Errors { get; }
    public bool ExplicitArguments { get; set; }

    public void AddArguments(IEnumerable<CommandArgument> arguments)
    {
        Debug.Assert(this.Arguments.Count == 0);

        foreach (var argument in arguments)
        {
            if (!this.ParametersSeen.Add(argument))
                continue;

            this.Arguments.Enqueue(argument);
        }
    }

    public void AddOptions(IEnumerable<CommandOption> options)
    {
        foreach (var option in options)
        {
            if (!this.ParametersSeen.Add(option))
                continue;

            foreach (var name in option.GetNames())
                this.Options.Add(name, option);
        }
    }

    public void AddParameters(CommandParametersInfo parametersInfo)
    {
        AddArguments(parametersInfo.Arguments);
        AddOptions(parametersInfo.Options);
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
}
