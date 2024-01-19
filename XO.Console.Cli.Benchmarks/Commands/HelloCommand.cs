namespace XO.Console.Cli.Commands;

internal class HelloCommand : AsyncCommand<HelloCommand.Parameters>
{
    public sealed class Parameters : CommandParameters
    {
        [CommandOption("--name", "-n")]
        public string? Name { get; set; } = "World";

        [CommandOption("--times", "-t")]
        public int Times { get; set; } = 1;
    }

    public override Task<int> ExecuteAsync(ICommandContext context, Parameters parameters, CancellationToken cancellationToken)
    {
        for (var i = 0; i < parameters.Times; i++)
        {
            context.Console.Output.WriteLine($"Hello, {parameters.Name}!");
        }
        return Task.FromResult(0);
    }
}
