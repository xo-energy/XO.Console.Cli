namespace XO.Console.Cli.Commands;

internal sealed class VersionCommand : AsyncCommand
{
    private readonly string _version;

    public VersionCommand(string version)
    {
        _version = version;
    }

    public override async Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
    {
        await context.Console.Output.WriteLineAsync(_version).ConfigureAwait(false);
        return 0;
    }
}
