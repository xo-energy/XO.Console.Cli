namespace XO.Console.Cli;

/// <summary>
/// Represents the syntax for command-line options.
/// </summary>
public enum CommandOptionStyle
{
    /// <summary>
    /// Option names are preceded by '<c>--</c>', or, for single-character names, '<c>-</c>'.
    /// </summary>
    Posix,

    /// <summary>
    /// Option names are preceded by '<c>/</c>'.
    /// </summary>
    Dos,
}

internal static class CommandOptionStyleExtensions
{
    public static StringComparer GetDefaultNameComparer(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Dos => StringComparer.OrdinalIgnoreCase,
            CommandOptionStyle.Posix => StringComparer.Ordinal,
            _ => throw new ArgumentOutOfRangeException(),
        };

    public static bool GetDefaultLeaderMustStartOption(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Dos => false,
            CommandOptionStyle.Posix => true,
            _ => throw new ArgumentOutOfRangeException(),
        };

    public static char GetDefaultValueSeparator(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Dos => ':',
            CommandOptionStyle.Posix => '=',
            _ => throw new ArgumentOutOfRangeException(),
        };

    public static char GetLeader(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Dos => '/',
            CommandOptionStyle.Posix => '-',
            _ => throw new ArgumentOutOfRangeException(),
        };

    public static string GetNameWithLeader(this CommandOptionStyle optionStyle, string name)
    {
        var leader = optionStyle.GetLeader();

        if (name.Length > 1 && optionStyle.HasShortOptions())
            return $"{leader}{leader}{name}";
        else
            return $"{leader}{name}";
    }

    public static bool HasShortOptions(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Posix => true,
            _ => false,
        };
}
