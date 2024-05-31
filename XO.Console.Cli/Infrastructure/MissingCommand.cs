namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// A command that is executed when the given arguments do not match any configured command.
/// </summary>
public sealed class MissingCommand : Command
{
    /// <inheritdoc/>
    public override int Execute(ICommandContext context, CancellationToken cancellationToken)
    {
        context.Console.Error.WriteLine("Use --help to see usage information");
        return 1;
    }
}
