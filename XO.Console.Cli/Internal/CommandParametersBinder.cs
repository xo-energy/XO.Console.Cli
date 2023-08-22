using System.Collections.Immutable;

namespace XO.Console.Cli;

internal sealed class CommandParametersBinder
{
    private readonly Dictionary<Type, Func<string, object?>> _converters;

    public CommandParametersBinder(IEnumerable<KeyValuePair<Type, Func<string, object?>>> converters)
    {
        _converters = new(converters);
    }

    public ImmutableDictionary<CommandParameter, object?> BindParameters(IEnumerable<CommandToken> tokens)
    {
        // group tokens by their parameter (some may have multiple values)
        var parameterTokens = GetTokensByParameter(tokens)
            .GroupBy(x => x.Parameter, x => x.Value);
        var parameterValues = ImmutableDictionary.CreateBuilder<CommandParameter, object?>();

        foreach (var group in parameterTokens)
        {
            var value = BindParameterValues(group.Key, group);
            parameterValues.Add(group.Key, value);
        }

        return parameterValues.ToImmutable();
    }

    private object? BindParameterValues(CommandParameter parameter, IEnumerable<string> values)
    {
        var valueCount = values.Count();
        var valueType = parameter.ValueType;

        // detect arrays and get the element type
        var isArray = valueType.IsArray;
        if (isArray)
        {
            valueType = parameter.ValueType.GetElementType()!;

            var array = Array.CreateInstance(valueType, valueCount);
            var arrayIndex = 0;
            var converter = GetConverter(valueType);
            object? converted;

            foreach (var value in values)
            {
                try
                {
                    converted = converter(value);
                    array.SetValue(converted, arrayIndex++);
                }
                catch (Exception ex)
                {
                    throw new CommandParameterBindingException(
                        $"Failed to convert value '{value}' to type '{valueType}'",
                        ex);
                }
            }

            return array;
        }
        else if (valueCount > 1)
        {
            throw new CommandParameterBindingException(
                $"Found multiple values for non-array property '{parameter.Name}' ({parameter.ValueType})");
        }
        else
        {
            var converter = GetConverter(valueType);
            var value = values.Single();
            try
            {
                return converter(value);
            }
            catch (Exception ex)
            {
                throw new CommandParameterBindingException(
                    $"Failed to convert value '{value}' to type '{valueType}'",
                    ex);
            }
        }
    }

    private Func<string, object?> GetConverter(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            return GetConverter(underlyingType);

        if (_converters.TryGetValue(type, out var converter))
            return converter;

        // TODO: support IParsable
        if (type.IsEnum)
        {
            converter = (value) => Enum.Parse(type, value);
        }
        else
        {
            converter = (value) => Convert.ChangeType(value, type);
        }

        _converters.Add(type, converter);
        return converter;
    }

    private static IEnumerable<(CommandParameter Parameter, string Value)> GetTokensByParameter(IEnumerable<CommandToken> tokens)
    {
        foreach (var token in tokens)
        {
            switch (token.TokenType)
            {
                case CommandTokenType.Argument when token.Context is CommandArgument argument:
                    yield return (argument, token.Value);
                    break;

                case CommandTokenType.Option when token.Context is CommandOption option && option.IsFlag:
                    yield return (option, Boolean.TrueString);
                    break;

                case CommandTokenType.OptionGroup when token.Context is IEnumerable<CommandOption> optionGroup:
                    foreach (var option in optionGroup)
                        yield return (option, Boolean.TrueString);
                    break;

                case CommandTokenType.OptionValue when token.Context is CommandOption option:
                    yield return (option, token.Value);
                    break;

                case CommandTokenType.Unknown:
                    yield return (Builtins.Arguments.Remaining, token.Value);
                    break;
            }
        }
    }
}
