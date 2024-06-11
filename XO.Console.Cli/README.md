# XO.Console.Cli

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
- Reflection free and trimmable via compile-time source generation of command and parameter type information

## Getting Started

### 1. Add a reference to XO.Console.Cli

```console
> dotnet add package XO.Console.Cli
```

### 2. Write a command

A command is defined by its implementation (usually a class, but delegate implementations are supported, too) and its parameters type.

```csharp
// for an async implementation, derive from AsyncCommand<TParameters>
class GreetingCommand : Command<GreetingCommand.Parameters>
{
    // all parameters types must derive from CommandParameters
    public class Parameters : CommandParameters
    {
        // use CommandArgumentAttribute to declare a positional parameter
        [CommandArgument(0, "name", Description = "Name of the person to greet")]
        public required string Name { get; set; }

        // use CommandOptionAttribute to declare a named option
        [CommandOption("--greeting", Description = "A custom greeting (defaults to 'Hello')")]
        public string Greeting { get; set; } = "Hello";
    }

    public override int Execute(ICommandContext context, Parameters parameters, CancellationToken cancellationToken)
    {
        // 'context' provides an abstraction of the standard streams; using it for i/o makes automated testing easier!
        context.Console.Output.WriteLine($"{parameters.Greeting}, {parameters.Name}!");

        // return the desired process exit code (conventionally, 0 = success, and any non-zero exit code = failure)
        return 0;
    }
}
```

### 3. Create an instance of `ICommandAppBuilder`

For a typical application with several possible commands, simply use the public constructor `new CommandAppBuilder()`.

```csharp
return await new CommandAppBuilder()
    .ExecuteAsync(args);
```

If you want your application to have a _default command_ — to do something useful when invoked without additional arguments — call the static factory method `CommandAppBuilder.WithDefaultCommand`:

```csharp
// there are overloads that accept a parameters class or a class command implementation, too!
return await CommandAppBuilder.WithDefaultCommand(
    async (context, cancellationToken) =>
    {
        await context.Console.Output.WriteLineAsync("Hello, world!");
        return 0;
    })
    .ExecuteAsync(args);
```

### 4. Add commands

Call the methods of `ICommandAppBuilder` to add commands to the application. Each command must have a unique "verb" that invokes it.

```csharp
return await new CommandAppBuilder()
    .AddCommand<GreetingCommand>("greet")
    .ExecuteAsync(args);
```

### 5. Run it

```console
> dotnet run
Use --help to see usage information

> dotnet run -- greet --help
XO.Console.Cli.Samples 1.0.0

USAGE
  XO.Console.Cli.Samples greet <name> [OPTIONS]

ARGUMENTS
  name    Name of the person to greet

OPTIONS
      --greeting    A custom greeting (defaults to 'Hello')

  -h, --help        Shows this help

> dotnet run -- greet John
Hello, John!

> dotnet run -- greet John --greeting Hey
Hey, John!
```

## Declarative Configuration

By decorating command classes with `CommandAttribute`, you can configure your application's commands declaratively, without writing out the calls to `ICommandBuilder`.

> The source generator outputs code that adds a command to every instance of `CommandAppBuilder` for each class decorated with `CommandAttribute`. If you want to build multiple instances of `ICommandApp` with different commands, stick to explicit, imperative configuration.

### Using `CommandAttribute`

Apply the attribute to your command class, passing the verb that will invoke the command as the first argument. Use the attribute's keyword parameters to configure other properties of the command, such as its description. Each class may have only one instance of `CommandAttribute`.

```csharp
[Command("greet", Description = "Prints a greeting")]
class GreetingCommand : Command<GreetingCommand.Parameters> { /* ... */ }
```

### Declarative Branches

You can group commands into branches using attributes, too. To create a branch, declare a sub-class of `CommandAttribute`, then decorate it with `CommandBranchAttribute`. `CommandBranchAttribute`'s positional parameter(s) define the "path" to the branch. A branch's "path" is the sequence of verbs that precede the final verb for each sub-command in the branch. In this example, the application has two commands:

- `get name` ⇒ `GetNameCommand`
- `get address` ⇒ `GetAddressCommand`

```csharp
[CommandBranch("get", Description = "Get values")]
class GetCommandAttribute : CommandAttribute
{
    public GetCommandAttribute(string verb)
        : base(verb) { }
}

[GetCommand("name")]
class GetNameCommand : Command { /* ... */ }

[GetCommand("address")]
class GetAddressCommand : Command { /* ... */ }
```

> Public constructors of your custom attribute(s) must have a first parameter of exactly `string verb`, and there must be at least one such constructor. The source generator reads the literal value of `verb` from your source code. An included analyzer will report an error if the constructor parameter is missing.

### Declarative Branches and Commands

By itself, `CommandBranchAttribute` doesn't declare any implementation for the branch. In the above example, if you were to pass the argument `get` without a sub-command, the application would print the standard usage notice. To assign a command class to the branch, decorate it with the _same verb_ as the branch.

```csharp
[Command("get", Description = "Get values")]
class GetCommand : Command { /* ... */ }
```

Notice we used the base `CommandAttribute` and moved the description from the branch declaration. When you assign an implementation to a branch, any properties assigned to its `CommandBranchAttribute` are ignored, and the properties of the `CommandAttribute` applied to the implementation class are used instead.

### Nested Declarative Branches

Passing multiple path segments to `CommandBranchAttribute` creates additional "levels" of sub-commands. For example, we could add a command that gets only part of the address.

```csharp
[CommandBranch("get", "address")]
class GetAddressCommandAttribute : CommandAttribute
{
    public GetAddressCommandAttribute(string verb)
        : base(verb) { }
}

// invoke this command with the args "get address state"
[GetAddressCommand("state")]
class GetAddressStateCommand : Command { /* ... */ }
```
