using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command-line application.
/// </summary>
public interface ICommandAppBuilder : ICommandBuilderAddCommand<ICommandAppBuilder>
{
    /// <summary>
    /// Builds the application.
    /// </summary>
    /// <returns>A new, configured instance of <see cref="ICommandApp"/>.</returns>
    ICommandApp Build();

    /// <summary>
    /// Adds an option that is accepted by all commands.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="description">The option description, to be included in generated help.</param>
    /// <param name="aliases">Any option aliases, including the option leader (prefix).</param>
    /// <typeparam name="TValue">The type of the option's value.</typeparam>
    ICommandAppBuilder AddGlobalOption<TValue>(
        string name,
        string description,
        params string[] aliases);

    /// <summary>
    /// Adds a parameter value converter.
    /// </summary>
    /// <param name="converter">A delegate that converts a parameter value to type <typeparamref name="TValue"/>.</param>
    /// <typeparam name="TValue">The parameter value type.</typeparam>
    ICommandAppBuilder AddParameterConverter<TValue>(Func<string, TValue> converter);

    /// <summary>
    /// Disables strict parsing.
    /// </summary>
    /// <remarks>
    /// When strict parsing is disabled, the application does not throw an exception when parsing errors are detected.
    /// Instead, any unknown arguments are passed to the command in <see cref="ICommandContext.RemainingArguments"/>.
    /// </remarks>
    ICommandAppBuilder DisableStrictParsing();

    /// <summary>
    /// Sets the application name, used in generated help.
    /// </summary>
    /// <remarks>
    /// The default is the name of the entry assembly, if any; otherwise, the filename of the process path.
    /// </remarks>
    /// <param name="name">The application name.</param>
    ICommandAppBuilder SetApplicationName(string name);

    /// <summary>
    /// Sets the application version, used in generated help.
    /// </summary>
    /// <remarks>
    /// The default is the entry assembly's <see cref="AssemblyInformationalVersionAttribute.InformationalVersion"/>,
    /// if any; otherwise, the entry assembly's <see cref="AssemblyName.Version"/>.
    /// </remarks>
    /// <param name="version">The application version.</param>
    ICommandAppBuilder SetApplicationVersion(string version);

    /// <summary>
    /// Sets the application description, which is displayed in generated help.
    /// </summary>
    /// <param name="description">The application description.</param>
    ICommandAppBuilder SetDescription(string description);

    /// <summary>
    /// Sets the option style.
    /// </summary>
    /// <param name="style">The option style.</param>
    /// <param name="optionLeaderMustStartOption">
    /// Sets whether any argument starting with the option leader character ('-' or '/') must be parsed as an option
    /// name. When set to <see langword="false"/>, arguments that look like an option name can be parsed as parameter
    /// values or commands. The default is <see langword="true"/> for <see cref="CommandOptionStyle.Posix"/> and
    /// <see langword="false"/> for <see cref="CommandOptionStyle.Dos"/>.
    /// </param>
    /// <param name="optionNameComparer">
    /// Sets the <see cref="StringComparer"/> to use when comparing option names. The default is
    /// <see cref="StringComparer.Ordinal"/> for <see cref="CommandOptionStyle.Posix"/> and
    /// <see cref="StringComparer.OrdinalIgnoreCase"/> for <see cref="CommandOptionStyle.Dos"/>.
    /// </param>
    /// <param name="optionValueSeparator">
    /// Sets the character that separates an option name from its value when passing both in the same argument. The
    /// default is <c>'='</c> for <see cref="CommandOptionStyle.Posix"/> and <c>':'</c> for
    /// <see cref="CommandOptionStyle.Dos"/>.
    /// </param>
    ICommandAppBuilder SetOptionStyle(
        CommandOptionStyle style,
        bool? optionLeaderMustStartOption = default,
        StringComparer? optionNameComparer = default,
        char? optionValueSeparator = default);

    /// <summary>
    /// Adds middleware to the end of the application pipeline that catches exceptions and writes the exception message
    /// to <c>stderr</c>.
    /// </summary>
    ICommandAppBuilder UseExceptionHandler();

    /// <summary>
    /// Adds a middleware delegate to the application pipeline.
    /// </summary>
    /// <param name="middleware">The middleware delegate.</param>
    ICommandAppBuilder UseMiddleware(Func<ExecutorDelegate, ExecutorDelegate> middleware);

    /// <summary>
    /// Adds a middleware to the application pipeline.
    /// </summary>
    /// <param name="middleware">The middleware implementation.</param>
    ICommandAppBuilder UseMiddleware(ICommandAppMiddleware middleware);

    /// <summary>
    /// Adds a middleware to the application pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware implementation type.</typeparam>
    ICommandAppBuilder UseMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>()
        where TMiddleware : ICommandAppMiddleware;

    /// <summary>
    /// Sets the <see cref="ITypeResolver"/> used to create instances of command and middleware implementations.
    /// </summary>
    /// <remarks>
    /// The default type resolver is <see cref="Activator"/>. Provide a custom type resolver to support dependency
    /// injection for commands and middleware.
    /// </remarks>
    /// <param name="resolver">An instance of <see cref="ITypeResolver"/>.</param>
    ICommandAppBuilder UseTypeResolver(ITypeResolver resolver);
}
