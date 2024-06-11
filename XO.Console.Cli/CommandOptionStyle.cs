using System.Text.RegularExpressions;

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

#if NET8_0_OR_GREATER
internal static partial class CommandOptionStyleExtensions
#else
internal static class CommandOptionStyleExtensions
#endif
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

    public static Regex GetNameValidationPattern(this CommandOptionStyle optionStyle)
        => optionStyle switch
        {
            CommandOptionStyle.Dos => GetOptionNamePatternDos(),
            CommandOptionStyle.Posix => GetOptionNamePatternPosix(),
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

    private const string OptionNamePatternDosInput = @"^\/[a-z0-9?][a-z0-9?_-]*$";
    private const string OptionNamePatternPosixInput = @"^(?:-[a-z0-9?]|--[a-z0-9?][a-z0-9?_-]*)$";
    private const RegexOptions OptionNamePatternRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

#if NET8_0_OR_GREATER
    [GeneratedRegex(OptionNamePatternDosInput, OptionNamePatternRegexOptions)]
    private static partial Regex GetOptionNamePatternDos();

    [GeneratedRegex(OptionNamePatternPosixInput, OptionNamePatternRegexOptions)]
    private static partial Regex GetOptionNamePatternPosix();
#else
    private static Regex GetOptionNamePatternDos()
        => new(OptionNamePatternDosInput, OptionNamePatternRegexOptions);

    private static Regex GetOptionNamePatternPosix()
        => new(OptionNamePatternPosixInput, OptionNamePatternRegexOptions);
#endif
}
