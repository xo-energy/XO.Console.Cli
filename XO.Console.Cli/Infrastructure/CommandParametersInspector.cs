using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Infrastructure;

internal sealed class CommandParametersInspector
{
    private readonly StringComparer _nameComparer;
    private readonly CommandOptionStyle _optionStyle;
    private readonly Dictionary<Type, CommandParametersInfo> _parametersByProperty;

    public CommandParametersInspector(
        CommandOptionStyle optionStyle = CommandAppDefaults.OptionStyle,
        StringComparer? optionNameComparer = null)
    {
        _nameComparer = optionNameComparer ?? optionStyle.GetDefaultNameComparer();
        _optionStyle = optionStyle;
        _parametersByProperty = new();
    }

    public CommandParametersInfo InspectParameters(ConfiguredCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // do we need to inspect this parameters type?
        if (!_parametersByProperty.TryGetValue(command.ParametersType, out var parametersInfo))
        {
            parametersInfo = InspectInternal(command.ParametersType);
            _parametersByProperty.Add(command.ParametersType, parametersInfo);
        }

        // check for an invalid combination of arguments with subcommands
        ValidateArgumentsForCommand(command, parametersInfo.Arguments);

        return parametersInfo;
    }

    private CommandParametersInfo InspectInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type parametersType)
    {
        var properties = parametersType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var arguments = ImmutableList.CreateBuilder<CommandArgument>();
        var options = ImmutableList.CreateBuilder<CommandOption>();

        // scan public instance properties
        foreach (var property in properties)
        {
            // detect our marker attributes
            var argumentAttribute = property.GetCustomAttribute<CommandArgumentAttribute>();
            var optionAttribute = property.GetCustomAttribute<CommandOptionAttribute>();
            var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();

            // having both attributes is not allowed
            if (argumentAttribute != null && optionAttribute != null)
            {
                throw new CommandTypeException(
                    parametersType,
                    $"Property '{property.Name}' is marked as both an argument and an option");
            }
            else if (argumentAttribute != null)
            {
                var argument = new CommandArgument(
                    argumentAttribute,
                    property.DeclaringType!,
                    property.PropertyType,
                    (context, value) => property.SetValue(context.Parameters, value),
                    descriptionAttribute?.Description);

                arguments.Add(argument);
            }
            else if (optionAttribute != null)
            {
                var option = new CommandOption(
                    optionAttribute,
                    property.DeclaringType!,
                    property.PropertyType,
                    (context, value) => property.SetValue(context.Parameters, value),
                    descriptionAttribute?.Description);

                options.Add(option);
            }
        }

        // arguments are ordered
        arguments.Sort((x, y) => x.Attribute.Order.CompareTo(y.Attribute.Order));

        // check for duplicate names
        ValidateNames(parametersType, arguments, options);

        // check for invalid combinations of arguments
        ValidateArguments(parametersType, arguments);

        return new CommandParametersInfo(
            arguments.ToImmutable(),
            options.ToImmutable());
    }

    public void ValidateNames(
        Type parametersType,
        IEnumerable<CommandArgument> arguments,
        IEnumerable<CommandOption> options)
    {
        var names = new HashSet<string>(_nameComparer);

        foreach (var argument in arguments)
        {
            if (!names.Add(argument.Name))
            {
                throw new CommandTypeException(
                    parametersType,
                    $"Duplicate argument name '{argument.Name}'");
            }
        }

        foreach (var option in options)
        {
            foreach (var name in option.GetNames())
            {
                ValidateOptionName(name, _optionStyle);
                if (!names.Add(name))
                {
                    throw new CommandTypeException(
                        parametersType,
                        $"Duplicate option name '{name}'");
                }
            }
        }
    }

    private void ValidateArguments(Type parametersType, IReadOnlyList<CommandArgument> arguments)
    {
        for (int i = 1; i < arguments.Count; ++i)
        {
            if (arguments[i - 1].Attribute.IsGreedy)
            {
                throw new CommandTypeException(
                    parametersType,
                    $"Argument '{arguments[i - 1]}' is greedy, but there are other arguments after it");
            }

            if (arguments[i - 1].Attribute.IsOptional && !arguments[i].Attribute.IsOptional)
            {
                throw new CommandTypeException(
                    parametersType,
                    $"Argument '{arguments[i]}' is required, but the previous argument was optional");
            }
        }
    }

    private void ValidateArgumentsForCommand(ConfiguredCommand command, IReadOnlyList<CommandArgument> arguments)
    {
        if (!command.Commands.Any())
            return;

        foreach (var argument in arguments)
        {
            if (argument.Attribute.IsGreedy)
            {
                throw new CommandTypeException(
                    command.ParametersType,
                    $"Command '{command.Verb}' has subcommands, but its argument '{argument.Name}' is greedy");
            }

            if (argument.Attribute.IsOptional)
            {
                throw new CommandTypeException(
                    command.ParametersType,
                    $"Command '{command.Verb}' has subcommands, but its argument '{argument.Name}' is optional");
            }
        }
    }

    private static void ValidateOptionName(string name, CommandOptionStyle optionStyle)
    {
        switch (optionStyle)
        {
            case CommandOptionStyle.Dos:
                ValidateOptionNameDos(name);
                break;
            case CommandOptionStyle.Posix:
                ValidateOptionNamePosix(name);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(optionStyle));
        }
    }

    private static void ValidateOptionNameDos(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var start = 1;

        // check length
        if (name.Length < 2)
        {
            throw new ArgumentException(
                $"Option name must contain a forward slash and at least one alphanumeric character: {name}");
        }

        // require initial slash to be included in the option name
        if (name[0] != '/')
            throw new ArgumentException($"Option name must begin with '/': {name}");

        // validate remainder of name
        for (var i = start; i < name.Length; i++)
        {
            switch (name[i])
            {
                case >= 'a' and <= 'z':
                case >= 'A' and <= 'Z':
                case >= '0' and <= '9':
                case '?':
                case '_' when i > start:
                case '-' when i > start:
                    break;

                default:
                    throw new ArgumentException($"Invalid option name: {name}");
            }
        }
    }

    private static void ValidateOptionNamePosix(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var start = 1;

        // check length for initial dash validation
        if (name.Length < 2 || name == "--")
        {
            throw new ArgumentException(
                $"Option name must contain at least one hyphen and one alphanumeric character: {name}");
        }

        // require initial dashes to be included in the option name
        if (name[0] != '-')
            throw new ArgumentException($"Option name must begin with '-': {name}");
        if (name[1] != '-' && name.Length > 2)
            throw new ArgumentException($"Long option name must begin with '--': {name}");
        if (name[1] == '-')
            start = 2;

        // validate remainder of name
        for (var i = start; i < name.Length; i++)
        {
            switch (name[i])
            {
                case >= 'a' and <= 'z':
                case >= 'A' and <= 'Z':
                case >= '0' and <= '9':
                case '_' when i > start:
                case '-' when i > start:
                    break;

                default:
                    throw new ArgumentException($"Invalid option name: {name}");
            }
        }
    }
}
