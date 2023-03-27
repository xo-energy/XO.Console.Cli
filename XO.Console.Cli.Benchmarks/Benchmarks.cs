using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Hosting;
using XO.Console.Cli.Commands;

namespace XO.Console.Cli;

[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.NativeAot70)]
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
    public ICommandApp InitializeApp()
    {
        return CommandAppBuilder.Create()
            .AddBranch("do", builder =>
            {
                builder.AddCommand<HelloCommand>();
                builder.AddCommand<GoodbyeCommand>();
            })
            .UseMiddleware(NullConsoleMiddleware)
            .Build();
    }

    [Benchmark]
    public async Task<int> RunApp()
    {
        return await CommandAppBuilder.Create()
            .AddBranch("do", builder =>
            {
                builder.AddCommand<HelloCommand>();
                builder.AddCommand<GoodbyeCommand>();
            })
            .UseMiddleware(NullConsoleMiddleware)
            .ExecuteAsync(Args)
            .ConfigureAwait(false);
    }

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
    public async Task<int> RunHostedApp()
    {
        return await new HostBuilder()
            .RunCommandAppAsync(Args, (_, builder) =>
            {
                builder.AddBranch("do", builder =>
                    {
                        builder.AddCommand<HelloCommand>();
                        builder.AddCommand<GoodbyeCommand>();
                    })
                    ;
                builder.UseMiddleware(NullConsoleMiddleware);
            })
            .ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<int> RunHostedCommandExplicitly()
    {
        using var host = new HostBuilder()
            .Build();

        try
        {
            await host.StartAsync()
                .ConfigureAwait(false);

            var command = new HelloCommand();
            var parameters = new HelloCommand.Parameters() { Name = "Frank", Times = 2 };
            var parseResult = new CommandParseResult(ImmutableArray<CommandToken>.Empty, ImmutableList<string>.Empty);
            var scope = new DefaultTypeResolverScope(DefaultTypeResolver.Instance);
            var context = new CommandContext(scope, command, parameters, parseResult) { Console = NullConsole.Instance };

            var result = await command
                .ExecuteAsync(context, parameters, default)
                .ConfigureAwait(false);

            await host.StopAsync()
                .ConfigureAwait(false);

            return result;
        }
        catch
        {
            return 1;
        }
    }
}
