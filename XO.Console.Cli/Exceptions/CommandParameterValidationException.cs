using System.ComponentModel.DataAnnotations;

namespace XO.Console.Cli;

/// <summary>
/// The exception that is thrown when <see cref="CommandParameters.Validate"/> does not succeed.
/// </summary>
public class CommandParameterValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParameterValidationException"/>.
    /// </summary>
    /// <param name="result">The <see cref="ValidationResult"/> that caused this exception.</param>
    public CommandParameterValidationException(ValidationResult result)
        : base(result.ToString())
    {
        this.ValidationResult = result;
    }

    /// <summary>
    /// Gets the <see cref="ValidationResult"/> that caused this exception.
    /// </summary>
    public ValidationResult ValidationResult { get; }
}
