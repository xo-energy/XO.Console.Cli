using System.Collections.Immutable;
using XO.Console.Cli.Infrastructure;

namespace XO.Console.Cli.Model;

internal sealed class CommandAppSettings
{
    public CommandAppSettings(
        string applicationName,
        string applicationVersion,
        ImmutableDictionary<Type, ParameterValueConverter> converters,
        ImmutableList<CommandOption> globalOptions)
    {
        this.ApplicationName = applicationName;
        this.ApplicationVersion = applicationVersion;
        this.Converters = converters;
        this.GlobalOptions = globalOptions;
    }

    public string ApplicationName { get; }

    public string ApplicationVersion { get; }

    public ImmutableDictionary<Type, ParameterValueConverter> Converters { get; }

    public ImmutableList<CommandOption> GlobalOptions { get; }

    public char OptionLeader { get; init; }
        = CommandAppDefaults.OptionStyle.GetLeader();

    public bool OptionLeaderMustStartOption { get; init; }
        = CommandAppDefaults.OptionStyle.GetDefaultLeaderMustStartOption();

    public StringComparer OptionNameComparer { get; init; }
        = CommandAppDefaults.OptionStyle.GetDefaultNameComparer();

    public CommandOptionStyle OptionStyle { get; init; }
        = CommandAppDefaults.OptionStyle;

    public char OptionValueSeparator { get; init; }
        = CommandAppDefaults.OptionStyle.GetDefaultValueSeparator();

    public bool Strict { get; init; }
        = CommandAppDefaults.Strict;
}
