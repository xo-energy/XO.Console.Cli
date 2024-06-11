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
- Reflection free and trimmable via compile-time source generation of command and parameter type information
- Dependency free

## Packages

Documentation for each package is available in its individual README file, which is linked in the "Package" column below.

| Package | | Description |
| - | - | - |
| [XO.Console.Cli](./XO.Console.Cli/README.md) | [![NuGet Version](https://img.shields.io/nuget/v/XO.Console.Cli)](https://www.nuget.org/packages/XO.Console.Cli/) | Core library (parser and application framework) |
| [XO.Console.Cli.Extensions](./XO.Console.Cli.Extensions/README.md) | [![NuGet Version](https://img.shields.io/nuget/v/XO.Console.Cli.Extensions)](https://www.nuget.org/packages/XO.Console.Cli.Extensions/) | Integration with hosting, logging, and dependency injection |
| [XO.Console.Cli.Instrumentation](./XO.Console.Cli.Instrumentation/README.md) | [![NuGet Version](https://img.shields.io/nuget/v/XO.Console.Cli.Instrumentation)](https://www.nuget.org/packages/XO.Console.Cli.Instrumentation/) | Integration with OpenTelemetry |

## Synopsis

```csharp
using XO.Console.Cli;

return await new CommandAppBuilder()
    .ExecuteAsync(args);

[Command("greet", Description = "Prints a greeting")]
class GreetingCommand : Command<GreetingCommand.Parameters>
{
    public class Parameters : CommandParameters
    {
        [CommandArgument(0, "name", Description = "Name of the person to greet")]
        public required string Name { get; set; }

        [CommandOption("--greeting", Description = "A custom greeting (defaults to 'Hello')")]
        public string Greeting { get; set; } = "Hello";
    }

    public override int Execute(ICommandContext context, Parameters parameters, CancellationToken cancellationToken)
    {
        context.Console.Output.WriteLine($"{parameters.Greeting}, {parameters.Name}!");
        return 0;
    }
}
```

## Acknowledgements

The design of this library's public API was partially inspired by [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console/tree/main/src/Spectre.Console.Cli). I liked that project's development model, but I wished its implementation were different, and here we are.
