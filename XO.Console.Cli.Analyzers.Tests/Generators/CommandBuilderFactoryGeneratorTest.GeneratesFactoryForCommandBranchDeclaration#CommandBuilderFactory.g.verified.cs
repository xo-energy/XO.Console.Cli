﻿//HintName: CommandBuilderFactory.g.cs
// <auto-generated/>
using XO.Console.Cli.Infrastructure;

#nullable enable

namespace XO.Console.Cli.Generated;

/// <summary>
/// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("XO.Console.Cli.Analyzers", "{ThisAssembly.AssemblyVersion}")]
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
internal sealed class CommandBuilderFactory : ICommandBuilderFactory
{
#pragma warning disable CS0436 // Type conflicts with imported type
    public static readonly CommandBuilderFactory Instance = new CommandBuilderFactory();
#pragma warning restore CS0436 // Type conflicts with imported type

    /// <inheritdoc/>
    public void AddCommandAttributeCommands(CommandBuilder builder)
    {
        builder.AddCommand(
            new ConfiguredCommand("group", static (_) => new MissingCommand(), typeof(CommandParameters)));
    }

    /// <inheritdoc/>
    public CommandBuilder? CreateCommandBuilder<TCommand>(string verb)
        where TCommand : class, ICommand
    {
        return null;
    }

    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Register()
    {
        TypeRegistry.RegisterCommandBuilderFactory(Instance);
    }
}
