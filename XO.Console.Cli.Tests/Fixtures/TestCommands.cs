using System.Threading;
using System.Threading.Tasks;

namespace XO.Console.Cli.Tests.Fixtures;

public static class TestCommands
{
    public sealed class NoOp : AsyncCommand
    {
        public override Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
