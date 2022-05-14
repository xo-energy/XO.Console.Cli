namespace XO.Console.Cli;

/// <summary>
/// Represents a delegate that creates instances of a command implementation.
/// </summary>
public delegate ICommand? CommandFactory(ITypeResolver resolver);
