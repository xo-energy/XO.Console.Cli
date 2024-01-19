using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace XO.Console.Cli;

public class ServiceProviderTypeResolverTest
{
    private static IHostBuilder CreateHostBuilder()
        => new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddScoped<TestScopedService>();
                services.AddScoped<TestScopedServiceAsync>();
            })
            .UseDefaultServiceProvider(configure =>
            {
                configure.ValidateOnBuild = true;
                configure.ValidateScopes = true;
            });

    [Fact]
    public async Task CanResolveScopedService()
    {
        using var host = CreateHostBuilder()
            .Build();

        var resolver = new ServiceProviderTypeResolver(host.Services);
        var app = CommandAppBuilder.WithDefaultCommand<TestScopedServiceCommand>()
            .UseTypeResolver(resolver)
            .Build();

        await using var context = app.Bind(Array.Empty<string>());

        var command = Assert.IsType<TestScopedServiceCommand>(context.Command);

        Assert.NotNull(command.Service);
    }

    [Fact]
    public async Task DisposesScopedService()
    {
        using var host = CreateHostBuilder()
            .Build();

        var resolver = new ServiceProviderTypeResolver(host.Services);
        var app = CommandAppBuilder.WithDefaultCommand<TestScopedServiceCommand>()
            .UseTypeResolver(resolver)
            .Build();

        TestScopedService service;

        await using (var context = app.Bind(Array.Empty<string>()))
        {
            var command = Assert.IsType<TestScopedServiceCommand>(context.Command);
            service = command.Service;
        }

        Assert.True(service.IsDisposed);
    }

    [Fact]
    public async Task DisposesAsyncScopedService()
    {
        using var host = CreateHostBuilder()
            .Build();

        var resolver = new ServiceProviderTypeResolver(host.Services);
        var app = CommandAppBuilder.WithDefaultCommand<TestScopedServiceAsyncCommand>()
            .UseTypeResolver(resolver)
            .Build();

        TestScopedServiceAsync service;

        await using (var context = app.Bind(Array.Empty<string>()))
        {
            var command = Assert.IsType<TestScopedServiceAsyncCommand>(context.Command);
            service = command.Service;
        }

        Assert.True(service.IsDisposed);
    }

    [Fact]
    public void ResolvesCommand()
    {
        using var host = new HostBuilder()
            .Build();

        var resolver = new ServiceProviderTypeResolver(host.Services);

        var command = Assert.IsType<TestCommand>(resolver.Get<TestCommand>());

        Assert.NotNull(command.ServiceProvider);
    }

    internal sealed class TestCommand : AsyncCommand
    {
        public TestCommand(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public override Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class TestScopedServiceCommand : AsyncCommand
    {
        public TestScopedServiceCommand(TestScopedService service)
        {
            Service = service;
        }

        public TestScopedService Service { get; }

        public override Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class TestScopedServiceAsyncCommand : AsyncCommand
    {
        public TestScopedServiceAsyncCommand(TestScopedServiceAsync service)
        {
            Service = service;
        }

        public TestScopedServiceAsync Service { get; }

        public override Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class TestScopedService : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    internal sealed class TestScopedServiceAsync : IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }
}
