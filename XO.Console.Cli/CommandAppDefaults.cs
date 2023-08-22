namespace XO.Console.Cli;

/// <summary>
/// Defines default values for <see cref="ICommandAppBuilder"/> settings.
/// </summary>
public static class CommandAppDefaults
{
    /// <summary>
    /// Gets the collection of default parameter value converters.
    /// </summary>
    public static readonly IReadOnlyList<KeyValuePair<Type, Func<string, object?>>> Converters
        = new KeyValuePair<Type, Func<string, object?>>[]
        {
            new(typeof(DateOnly), static (value) => DateOnly.Parse(value)),
            new(typeof(DateTimeOffset), static (value) => DateTimeOffset.Parse(value)),
            new(typeof(DirectoryInfo), static(value) => new DirectoryInfo(value)),
            new(typeof(FileInfo), static (value) => new FileInfo(value)),
            new(typeof(Guid), static (value) => Guid.Parse(value)),
            new(typeof(TimeOnly), static (value) => TimeOnly.Parse(value)),
            new(typeof(TimeSpan), static (value) => TimeSpan.Parse(value)),
            new(typeof(Uri), static (value) => new Uri(value)),
        };

    /// <summary>
    /// Gets the default option style, which determines the required leading character(s) in options names.
    /// </summary>
    public const CommandOptionStyle OptionStyle = CommandOptionStyle.Posix;

    /// <summary>
    /// Gets whether command parsing is strict by default (i.e. whether to throw an exception for unknown or missing
    /// arguments).
    /// </summary>
    public const bool Strict = true;
}
