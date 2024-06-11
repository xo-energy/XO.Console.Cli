# XO.Console.Cli.Extensions

This library integrates XO.Console.Cli applications with the .NET runtime extensions, including dependency injection and the [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host). By default, the integration:

- Adds `CommandAppBuilderOptions` and `ICommandApp` to the service collection
- Configures XO.Console.Cli to create instances of command and parameters types using `IServiceProvider` (which means you can use constructor injection for dependencies)
- Sets the command-line application name to `IHostEnvironment.ApplicationName`
- Wraps `IHost` startup and command execution to ensure the application reliably logs critical exceptions and responds to shutdown signals

## Getting Started

### 1. Add a reference

```console
> dotnet add package XO.Console.Cli.Extensions
```

> If your project does not already reference Microsoft.Extensions.Hosting, add that, too.
>
> ```console
> > dotnet add package Microsoft.Extensions.Hosting
> ```

### 2. Set up and run the host

```csharp
return await Host.CreateDefaultBuilder(args)
    .RunCommandAppAsync(args, static (context, builder) => {
        builder.AddCommand<GreetingCommand>("greet");
    });
```

Integration with the simpler `IHostApplicationBuilder` is also possible. First, add the command-line application to the service collection. Then, build the host. Finally, run the configured command-line application.

```csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCommandApp(static (context, builder) => {
    builder.AddCommand<GreetingCommand>("greet");
});

IHost host = builder.Build();
return await host.RunCommandAppAsync(args);
```
