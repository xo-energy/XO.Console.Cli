# XO.Console.Cli

[![GitHub Actions Status](https://img.shields.io/github/actions/workflow/status/xo-energy/XO.Console.Cli/ci.yml?branch=main&logo=github)](https://github.com/xo-energy/XO.Console.Cli/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/xo-energy/XO.Console.Cli/branch/main/graph/badge.svg?token=07Z4JPQ27M)](https://codecov.io/gh/xo-energy/XO.Console.Cli)

**XO.Console.Cli** is a command line parser and application framework.

## Features

- Simple programs are simple
- Class or delegate command implementations
- Default commands, nested sub-commands ("branches"), global options, greedy arguments, and more
- Automatic help text
- Explicit argument value separator (`--`) forces parsing remainder of the command line as argument values (not commands or options)
- Built-in `--cli-explain` flag helps debug parsing
- Console abstraction makes commands testable
- Dependency free
- Optional integration with the [.NET Generic Host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host) via the companion library **XO.Console.Cli.Extensions**

## Usage

1. Use the static factory methods of `XO.Console.Cli.CommandAppBuilder` to create an instance of `ICommandAppBuilder`.
1. Add commands to the application using `ICommandAppBuilder`'s methods.
1. Call `ExecuteAsync()` to build and run the configured application.

### Default Command (delegate implementation)

This is the simplest possible application: a delegate default command that does not take any parameters. A "default command" is the command that runs when the arguments do not include a command name. (When there is no default command, invoking the program without arguments is an error.)

```csharp
using XO.Console.Cli;

return await CommandAppBuilder.WithDefaultCommand<CommandParameters>(
    async (context, parameters, cancellationToken) =>
    {
        await context.Console.Output.WriteLineAsync("Hello, world!");
        return 0;
    })
    .ExecuteAsync(args);
```

### Default Command (delegate implementation with arguments)

Configure the parameters a command accepts by defining a subclass of `CommandParameters` and decorating its properties with the `CommandArgument` and `CommandOption` attributes.

- Arguments are positional
  - By default, a value is required
  - Any optional argument must be positioned after all required arguments
  - A "greedy" argument consumes all remaining argument values in the command line and must be the last argument
- Options are passed by name
  - An option's value follows its name, as either the next argument (`--name value`) or separated by the option value separator (`--name=value`)
  - Options may have aliases, which allows you to configure a "short" version of the option name
  - The default POSIX style requires multi-character option names to begin with `--` and single-character option names to begin with `-` (DOS-style `/Options` are supported as an application-level switch)
  - Options bound to boolean-valued properties are interpreted as "flags": passing the option name sets its value to `true`, and a value argument is not accepted
  - Set array-valued options by passing the option multiple times, including the option name each time (`--input file1.txt --input file2.txt`)
- Override `CommandParameters.Validate()` to configure custom parameter validation

```csharp
using XO.Console.Cli;

return await CommandAppBuilder.WithDefaultCommand<HelloParameters>(
    async (context, parameters, cancellationToken) =>
    {
        await context.Console.Output.WriteLineAsync($"Hello, {parameters.Name}!");
        return 0;
    })
    .ExecuteAsync(args);

internal class HelloParameters : CommandParameters
{
    [CommandArgument(0, "name")]
    public string Name { get; set; }
}
```

### Default Command (class implementation with arguments)

Command classes inherit one of these base classes:

- `AsyncCommand<TParameters>` — asynchronous command implementation with parameters
- `AsyncCommand` — asynchronous command implementation without parameters
- `Command<TParameters>` — synchronous command implementation with parameters
- `Command` — synchronous command implementation without parameters

```csharp
using XO.Console.Cli;

return await CommandAppBuilder.WithDefaultCommand<HelloCommand>()
    .ExecuteAsync(args);

class HelloCommand : AsyncCommand<HelloCommand.Parameters>
{
    public class Parameters : CommandParameters
    {
        [CommandArgument(0, "name")]
        public string Name { get; set; }
    }

    public override Task<int> ExecuteAsync(ICommandContext context, Parameters parameters, CancellationToken cancellationToken)
    {
        context.Console.Output.WriteLine($"Hello, {parameters.Name}!");
        return Task.FromResult(0);
    }
}
```

### Multiple Commands

This example demonstrates several additional features:

- **Microsoft.Extensions.Hosting** integration with the `RunCommandAppAsync()` extension method
- Automatic discovery of public command classes decorated with the `Command` attribute
- Help text generated from the `Description` attribute
- Commands share a parameters model
- Constructor dependency injection support for commands (hosting integration automatically wires up `IServiceProvider`, but the user can supply a custom implementation of `ITypeResolver` to support other frameworks)

```csharp
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XO.Console.Cli;

return await new HostBuilder()
    .ConfigureServices(services => services.AddTransient<Greeter>())
    .RunCommandAppAsync(args, builder => builder.AddCommandsFromThisAssembly());

public class NameParameters : CommandParameters
{
    [CommandArgument(0, "name"), Description("The name of the person to greet")]
    public string Name { get; set; }
}

[Command("hello"), Description("Says 'Hello'")]
public class HelloCommand : AsyncCommand<NameParameters>
{
    private readonly Greeter _greeter;

    public HelloCommand(Greeter greeter)
    {
        _greeter = greeter;
    }

    public override async Task<int> ExecuteAsync(ICommandContext context, NameParameters parameters, CancellationToken cancellationToken)
    {
        await _greeter.GreetAsync(context.Console.Output, "Hello", parameters.Name);
        return 0;
    }
}

[Command("goodbye"), Description("Says 'Goodbye'")]
public class GoodbyeCommand : AsyncCommand<NameParameters>
{
    private readonly Greeter _greeter;

    public GoodbyeCommand(Greeter greeter)
    {
        _greeter = greeter;
    }

    public override async Task<int> ExecuteAsync(ICommandContext context, NameParameters parameters, CancellationToken cancellationToken)
    {
        await _greeter.GreetAsync(context.Console.Output, "Goodbye", parameters.Name);
        return 0;
    }
}

public class Greeter
{
    public async Task GreetAsync(TextWriter writer, string greeting, string name)
    {
        await writer.WriteLineAsync($"{greeting}, {name}!");
    }
}
```

Output from the above program with the `--help` flag:

```txt
XO.Console.Cli.Samples 1.0.0

USAGE
  XO.Console.Cli.Samples [OPTIONS] COMMAND

COMMANDS
  goodbye    Says 'Goodbye'
  hello      Says 'Hello'

OPTIONS
  -c, --configuration    Adds an additional configuration file
      --environment      Sets the hosting environment

  -h, --help             Shows this help
      --version          Shows the application version
```

## Acknowledgements

The design of this library's public API was partially inspired by [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console/tree/main/src/Spectre.Console.Cli). I liked that project's development model, but I wished its implementation were different, and here we are.
