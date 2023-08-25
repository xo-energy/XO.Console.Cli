namespace XO.Console.Cli;

/// <summary>
/// Configures a class as a command-line command.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandAttribute : Attribute
{
}
