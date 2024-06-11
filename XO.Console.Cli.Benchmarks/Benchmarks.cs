using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.NativeAot;
using Microsoft.Extensions.Hosting;
using XO.Console.Cli.Commands.Greeting;
using XO.Console.Cli.Implementation;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

[Config(typeof(Config))]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
public class Benchmarks
{
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddJob([
                new("net6.0", Job.Default)
                {
                    Infrastructure = { Toolchain = CsProjCoreToolchain.NetCoreApp60 },
                },
                new("net8.0", Job.Default)
                {
                    Infrastructure = { Toolchain = CsProjCoreToolchain.NetCoreApp80 },
                },
                new("net8.0-aot", Job.Default)
                {
                    Infrastructure = { Toolchain = NativeAotToolchain.Net80 },
                },
                new("dry-net6.0", Job.Dry)
                {
                    Run = { LaunchCount = 20 },
                    Infrastructure = { Toolchain = CsProjCoreToolchain.NetCoreApp60 },
                },
                new("dry-net8.0", Job.Dry)
                {
                    Run = { LaunchCount = 20 },
                    Infrastructure = { Toolchain = CsProjCoreToolchain.NetCoreApp80 },
                },
                new("dry-net8.0-aot", Job.Dry)
                {
                    Run = { LaunchCount = 20 },
                    Infrastructure = { Toolchain = NativeAotToolchain.Net80 },
                },
            ]);
        }
    }

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
        "greeting",
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
                builder.UseMiddleware(NullConsoleMiddleware);
            })
            .ConfigureAwait(false);
    }
}
