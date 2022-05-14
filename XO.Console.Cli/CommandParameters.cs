using System.ComponentModel.DataAnnotations;

namespace XO.Console.Cli;

/// <summary>
/// The base class for types representing parameters to a command-line command.
/// </summary>
public class CommandParameters
{
    /// <summary>
    /// When overridden in a derived class, validates the values bound to these parameters.
    /// </summary>
    /// <returns>A <see cref="ValidationResult"/> describing the result of validation.</returns>
    public virtual ValidationResult Validate()
    {
        return ValidationResult.Success!;
    }
}
