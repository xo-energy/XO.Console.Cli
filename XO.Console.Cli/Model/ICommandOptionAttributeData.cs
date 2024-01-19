using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

/// <summary>
/// Defines the properties that can by configured by <see cref="CommandOptionAttribute"/>.
/// </summary>
public interface ICommandOptionAttributeData
{
    /// <summary>
    /// The option name, including the option leader (prefix).
    /// </summary>
    /// <example>--option</example>
    public string Name { get; }

    /// <summary>
    /// A collection of option aliases, including the option leader (prefix).
    /// </summary>
    public ImmutableArray<string> Aliases { get; }

    /// <summary>
    /// Describes the option in generated help.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Indicates whether this option should be hidden from generated help.
    /// </summary>
    public bool IsHidden { get; }
}
