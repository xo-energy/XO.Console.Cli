# XO.Console.Cli.Instrumentation

This library instruments XO.Console.Cli applications for OpenTelemetry tracing.

## Getting Started

### 1. Add a reference

```console
> dotnet add package XO.Console.Cli.Instrumentation
```

> If your project does not already reference OpenTelemetry, add that, too.
>
> ```console
> > dotnet add package OpenTelemetry
> > dotnet add package OpenTelemetry.Extensions.Hosting
> ```

### 2. Configure the Activity Source

```csharp
return await Host.CreateDefaultBuilder(args)
    .ConfigureServices(static (context, services) => {
        services.AddOpenTelemetry()
            .WithTracing(static (tracerProviderBuilder) => {
                // add the XO.Console.Cli activity source to the tracer configuration
                tracerProviderBuilder.AddCommandAppInstrumentation();
            });
    })
    .RunCommandAppAsync(args);
```
