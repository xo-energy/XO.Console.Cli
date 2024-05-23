﻿//HintName: CommandBuilderFactory.g.cs
// <auto-generated/>
using System;
using System.Diagnostics.CodeAnalysis;
using XO.Console.Cli;
using XO.Console.Cli.Infrastructure;

#nullable enable

namespace XO.Console.Cli.Generated;

/// <summary>
/// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("XO.Console.Cli.Analyzers", "5.0.0.0")]
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
internal sealed class CommandBuilderFactory : ICommandBuilderFactory
{
#pragma warning disable CS0436 // Type conflicts with imported type
    public static readonly CommandBuilderFactory Instance = new CommandBuilderFactory();
#pragma warning restore CS0436 // Type conflicts with imported type

    public void ConfigureCommandApp(ICommandAppBuilder builder)
    {
        builder.AddBranch("get", builder => {
            builder.AddBranch("widgets", builder => {
                builder.AddCommand<Test.Command1>("command1");
            });
        });
    }

    public CommandBuilder? CreateCommandBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(string verb)
        where TCommand : class, ICommand
    {
        if (typeof(TCommand) == typeof(Test.Command1))
        {
            return new CommandBuilder<Test.Command1, XO.Console.Cli.CommandParameters>(
                verb,
                static (resolver) => resolver.Get<Test.Command1>());
        }

        return null;
    }

    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Register()
    {
        TypeRegistry.RegisterCommandBuilderFactory(Instance);
    }
}