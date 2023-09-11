namespace XO.Console.Cli.Infrastructure;

public sealed class ParameterValueConverter<TValue> : ParameterValueConverter
{
    private readonly Func<string, TValue> _converter;

    public ParameterValueConverter(Func<string, TValue> converter)
    {
        _converter = converter;
    }

    public Func<string, TValue> Convert
        => _converter;

    public override object? ConvertUntyped(string value)
        => _converter(value);

    public override Type ValueType
        => typeof(TValue);
}

public abstract class ParameterValueConverter
{
    public abstract object? ConvertUntyped(string value);

    public abstract Type ValueType { get; }

    public static ParameterValueConverter<TValue> FromDelegate<TValue>(Func<string, TValue> converter)
        => new(converter);
}
