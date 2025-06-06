namespace XO.Console.Cli.Fixtures;

public sealed class TestCommandWithParameters : AsyncCommand<TestParameters.Argument>
{
    public override Task<int> ExecuteAsync(ICommandContext context, TestParameters.Argument parameters, CancellationToken cancellationToken)
    {
        if (parameters.Arg == null)
        {
            return Task.FromResult(1);
        }

        context.Console.Output.WriteLine(parameters.Arg);
        return Task.FromResult(0);
    }
}
