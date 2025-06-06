using System.Collections.Immutable;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a property as a command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CommandOptionAttribute : Attribute, ICommandOptionAttributeData
{
    /// <summary>
    /// Creates a new instance of <see cref="CommandOptionAttribute"/>.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="aliases">A collection of option aliases, including the option leader (prefix).</param>
    public CommandOptionAttribute(string name, params string[] aliases)
    {
        this.Name = name;
        this.Aliases = ImmutableArray.Create(aliases);
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public ImmutableArray<string> Aliases { get; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public bool IsHidden { get; set; }

    /// <inheritdoc/>
    public bool IsRequired { get; set; }
}
