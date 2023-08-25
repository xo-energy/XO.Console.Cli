namespace XO.Console.Cli.Commands;

internal sealed class MissingCommand<TParameters> : Command<TParameters>
    where TParameters : CommandParameters
{
    public override int Execute(ICommandContext context, TParameters parameters, CancellationToken cancellationToken)
    {
        context.Console.Error.WriteLine("Use --help to see usage information");
        return 1;
    }
}
