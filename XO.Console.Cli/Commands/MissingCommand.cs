namespace XO.Console.Cli.Commands;

internal sealed class MissingCommand : Command
{
    public override int Execute(ICommandContext context, CancellationToken cancellationToken)
    {
        context.Console.Error.WriteLine("Use --help to see usage information");
        return 1;
    }
}
