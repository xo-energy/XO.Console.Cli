namespace XO.Console.Cli;

internal static class TestCommandAppBuilderExtensions
{
    public static ICommandAppBuilder UseConsole(this ICommandAppBuilder builder, IConsole console)
    {
        return builder
            .UseMiddleware(
                next =>
                {
                    return (context, cancellationToken) =>
                    {
                        context.Console = console;
                        return next(context, cancellationToken);
                    };
                });
    }
}
