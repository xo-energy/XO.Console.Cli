using XO.Console.Cli.Model;

namespace XO.Console.Cli;

internal sealed class MiddlewareAdapter
{
    private readonly ICommandAppMiddleware _middleware;
    private readonly ExecutorDelegate _next;

    public MiddlewareAdapter(ICommandAppMiddleware middleware, ExecutorDelegate next)
    {
        _middleware = middleware;
        _next = next;
    }

    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        => _middleware.ExecuteAsync(_next, context, cancellationToken);
}
