using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Hosting;
using XO.Console.Cli.Commands;
using XO.Console.Cli.Implementation;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[DryJob(RuntimeMoniker.Net60)]
[DryJob(RuntimeMoniker.Net80)]
[DryJob(RuntimeMoniker.NativeAot80)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
public class Benchmarks
{
    private sealed class NullConsole : IConsole
    {
        public TextReader Input => TextReader.Null;

        public TextWriter Output => TextWriter.Null;

        public TextWriter Error => TextWriter.Null;

        public bool IsInputRedirected => false;

        public bool IsOutputRedirected => false;

        public bool IsErrorRedirected => false;

        public static readonly NullConsole Instance = new NullConsole();
    }

    private static readonly Func<ExecutorDelegate, ExecutorDelegate> NullConsoleMiddleware
        = static next => (context, cancellationToken) =>
        {
            context.Console = NullConsole.Instance;
            return next(context, cancellationToken);
        };

    private static readonly string[] Args = new[] {
        "do",
        "hello",
        "--times=2",
        "--name",
        "Frank",
    };

    [Benchmark]
    public async Task<int> RunCommandExplicitly()
    {
        var command = new HelloCommand();
        var parameters = new HelloCommand.Parameters() { Name = "Frank", Times = 2 };
        var parseResult = new CommandParseResult(ImmutableArray<CommandToken>.Empty, ImmutableList<string>.Empty);
        var scope = new DefaultTypeResolverScope(DefaultTypeResolver.Instance);
        var context = new CommandContext(scope, command, parameters, parseResult) { Console = NullConsole.Instance };

        return await command
            .ExecuteAsync(context, parameters, default)
            .ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<int> RunApp()
    {
        return await new CommandAppBuilder()
            .AddBranch("do", builder =>
            {
                builder.AddCommand<HelloCommand>("hello");
                builder.AddCommand<GoodbyeCommand>("goodbye");
            })
            .UseMiddleware(NullConsoleMiddleware)
            .ExecuteAsync(Args)
            .ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<int> RunHostedApp()
    {
        return await new HostBuilder()
            .RunCommandAppAsync(Args, (_, builder) =>
            {
                builder.AddBranch("do", builder =>
                    {
                        builder.AddCommand<HelloCommand>("hello");
                        builder.AddCommand<GoodbyeCommand>("goodbye");
                    })
                    ;
                builder.UseMiddleware(NullConsoleMiddleware);
            })
            .ConfigureAwait(false);
    }
}
