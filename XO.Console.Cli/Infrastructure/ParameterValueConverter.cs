namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Defines a conversion of string parameter values to a specific type.
/// </summary>
/// <typeparam name="TValue">The type of value this converter produces.</typeparam>
/// <param name="input">The string value to convert.</param>
public delegate TValue ParameterValueConverter<TValue>(string input);

/// <summary>
/// Defines utility methods for parsing command parameter values.
/// </summary>
public static class ParameterValueConverter
{
    /// <inheritdoc cref="ConvertArray{TValue}(IEnumerable{string}, IReadOnlyDictionary{Type, Delegate}, ParameterValueConverter{TValue}, int)"/>
    public static TValue[] ConvertArray<TValue>(
        IEnumerable<string> values,
        IReadOnlyDictionary<Type, Delegate> converters,
        ParameterValueConverter<TValue> defaultConverter)
    {
        if (values.TryGetNonEnumeratedCount(out var count))
            return ConvertArray<TValue>(values, converters, defaultConverter, count);

        var output = new List<TValue>();
        var converter = defaultConverter;

        // support runtime-configured converters
        if (converters.TryGetValue(typeof(TValue), out var abstractConverter))
            converter = (ParameterValueConverter<TValue>)abstractConverter;

        foreach (var value in values)
        {
            output.Add(converter(value));
        }

        return output.ToArray();
    }

    /// <summary>
    /// Converts a sequence of string values to an array of values of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The target value type.</typeparam>
    /// <param name="values">The sequence of strings to convert.</param>
    /// <param name="converters">A collection of runtime-configured converters. When this collection contains the key <typeparamref name="TValue"/>, the associated converter is used instead of <paramref name="defaultConverter"/>.</param>
    /// <param name="defaultConverter">The default conversion from <see cref="string"/> to <typeparamref name="TValue"/> defined at compile time.</param>
    /// <param name="count">The number of elements in <paramref name="values"/>.</param>
    /// <returns>An array of <typeparamref name="TValue"/> with length equal to the number of elements in <paramref name="values"/>.</returns>
    public static TValue[] ConvertArray<TValue>(
        IEnumerable<string> values,
        IReadOnlyDictionary<Type, Delegate> converters,
        ParameterValueConverter<TValue> defaultConverter,
        int count)
    {
        int i = 0;
        var output = new TValue[count];
        var converter = defaultConverter;

        // support runtime-configured converters
        if (converters.TryGetValue(typeof(TValue), out var abstractConverter))
            converter = (ParameterValueConverter<TValue>)abstractConverter;

        foreach (var value in values)
        {
            output[i++] = converter(value);
        }

        return output;
    }

    /// <summary>
    /// Converts a singleton sequence of string values to a value of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The target value type.</typeparam>
    /// <param name="values">The sequence of strings to convert, which must contain exactly one element.</param>
    /// <param name="converters">A collection of runtime-configured converters. When this collection contains the key <typeparamref name="TValue"/>, the associated converter is used instead of <paramref name="defaultConverter"/>.</param>
    /// <param name="defaultConverter">The default conversion from <see cref="string"/> to <typeparamref name="TValue"/> defined at compile time.</param>
    /// <returns>The resulting <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="values"/> is empty</exception>
    /// <exception cref="ArgumentException"><paramref name="values"/> contains more than one element</exception>
    public static TValue ConvertSingle<TValue>(
        IEnumerable<string> values,
        IReadOnlyDictionary<Type, Delegate> converters,
        ParameterValueConverter<TValue> defaultConverter)
    {
        using var enumerator = values.GetEnumerator();

        if (!enumerator.MoveNext())
            throw new InvalidOperationException("values should not be empty");

        var value = enumerator.Current;
        var converter = defaultConverter;

        // support runtime-configured converters
        if (converters.TryGetValue(typeof(TValue), out var abstractConverter))
            converter = (ParameterValueConverter<TValue>)abstractConverter;

        if (enumerator.MoveNext())
            throw new ArgumentException("Parameter does not support multiple values", nameof(values));

        return converter(value);
    }

    /// <summary>
    /// Creates a new intance of <see cref="ParameterValueConverter{TValue}"/> from a delegate.
    /// </summary>
    /// <typeparam name="TValue">The type of value the converter will produce.</typeparam>
    /// <param name="converter">A delegate that implements the conversion.</param>
    /// <returns>A new instance of <see cref="ParameterValueConverter{TValue}"/>.</returns>
    public static KeyValuePair<Type, Delegate> FromDelegate<TValue>(ParameterValueConverter<TValue> converter)
        => new(typeof(TValue), converter);

    /// <summary>
    /// Converts a string to a single character.
    /// </summary>
    /// <param name="value">The string value, which must have length of exactly one.</param>
    /// <returns>The character value.</returns>
    /// <exception cref="CommandParameterBindingException"><paramref name="value"/> is the wrong length</exception>
    public static char ParseChar(string value)
    {
        if (value.Length == 1)
            return value[0];

        throw new CommandParameterBindingException(
            $"Failed to convert value '{value}' to type 'char'");
    }

    /// <summary>
    /// Converts a string to an enumeration value.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="value">The string value.</param>
    /// <returns>The result of <see cref="Enum.Parse{TEnum}(string, bool)"/>.</returns>
    public static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct
        => Enum.Parse<TEnum>(value, true);

    /// <summary>
    /// Throws an exception when no the source generator fails to generate a converter for values of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The target value type.</typeparam>
    /// <param name="value">The string value (unused).</param>
    public static TValue ParseNone<TValue>(string value)
        => throw new InvalidOperationException($"No converter found for parameter value type '{typeof(TValue)}'");
}
