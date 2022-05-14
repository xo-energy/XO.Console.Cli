using System.Reflection;
using XO.Console.Cli.Commands;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for <see cref="ICommandAppBuilder"/>.
/// </summary>
public static class CommandAppBuilderExtensions
{
    /// <summary>
    /// Adds commands from an assembly to the application using reflection.
    /// </summary>
    /// <remarks>
    /// Adds all public, non-nested types decorated with <see cref="CommandAttribute"/> to the command-line application.
    /// </remarks>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to configure.</param>
    /// <param name="assembly">The assembly to search for commands.</param>
    public static ICommandAppBuilder AddCommandsFromAssembly(
        this ICommandAppBuilder builder,
        Assembly assembly)
    {
        var addCommandGenericMethod =
            new Func<string, Action<ICommandBuilder>, ICommandAppBuilder>(
                    builder.AddCommand<MissingCommand>)
                .Method
                .GetGenericMethodDefinition();
        var addCommandArgs = new object?[2];

        // consider all public, non-nested types decorated with CommandAttribute
        var candidates = (
            from type in assembly.GetTypes()
            where !type.IsNested && type.IsPublic
            let attribute = type.GetCustomAttribute<CommandAttribute>()
            where attribute != null
            select (Type: type, Attribute: attribute));

        foreach (var (type, attribute) in candidates)
        {
            addCommandArgs[0] = attribute.Verb;
            addCommandGenericMethod.MakeGenericMethod(type)
                .Invoke(builder, addCommandArgs);
        }

        return builder;
    }

    /// <summary>
    /// Adds commands from the entry assembly to the application using reflection.
    /// </summary>
    /// <remarks>
    /// Adds all public, non-nested types decorated with <see cref="CommandAttribute"/> to the command-line application.
    /// </remarks>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to configure.</param>
    public static ICommandAppBuilder AddCommandsFromEntryAssembly(
        this ICommandAppBuilder builder)
    {
        var assembly = Assembly.GetEntryAssembly();

        if (assembly == null)
            throw new InvalidOperationException($"{nameof(Assembly.GetEntryAssembly)} returned null");

        return AddCommandsFromAssembly(builder, assembly);
    }

    /// <summary>
    /// Adds commands from the calling assembly to the application using reflection.
    /// </summary>
    /// <remarks>
    /// Adds all public, non-nested types decorated with <see cref="CommandAttribute"/> to the command-line application.
    /// </remarks>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to configure.</param>
    public static ICommandAppBuilder AddCommandsFromThisAssembly(
        this ICommandAppBuilder builder)
    {
        return AddCommandsFromAssembly(
            builder,
            Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Builds the <see cref="ICommandApp"/> and executes a command.
    /// </summary>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to build.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the exit code of the command.</returns>
    public static async Task<int> ExecuteAsync(
        this ICommandAppBuilder builder,
        IReadOnlyList<string> args,
        CancellationToken cancellationToken = default)
    {
        var app = builder.Build();

        return await app.ExecuteAsync(
            args,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
