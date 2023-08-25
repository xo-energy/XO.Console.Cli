﻿//HintName: CommandParametersFactory.g.cs
// <auto-generated/>
using System;
using System.Collections.Immutable;
using XO.Console.Cli;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;
using static XO.Console.Cli.Infrastructure.ParameterValueConverter;

#nullable enable

namespace XO.Console.Cli.Generated;

/// <summary>
/// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("XO.Console.Cli.Analyzers", "5.0.0.0")]
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
internal sealed class CommandParametersFactory : ICommandParametersFactory
{
#pragma warning disable CS0436 // Type conflicts with imported type
    public static readonly CommandParametersFactory Instance = new CommandParametersFactory();
#pragma warning restore CS0436 // Type conflicts with imported type

    public CommandParametersInfo? DescribeParameters(Type parametersType)
    {
        if (parametersType is null)
            throw new ArgumentNullException(nameof(parametersType));

        if (parametersType == typeof(Test.Wrapper.Parameters))
        {
            return new CommandParametersInfo(
                ImmutableArray.Create<CommandArgument>(
                    new CommandArgument(
                        "name",
                        static (context, values, converters) => ((Test.Wrapper.Parameters)context.Parameters).Name = ConvertSingle<System.String>(values, converters, static (value) => value),
                        typeof(System.String),
                        null)
                    {
                        Order = 1,
                        IsGreedy = false,
                        IsOptional = false,
                    }),
                ImmutableArray<CommandOption>.Empty);
        }

        return null;
    }

    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Register()
    {
        TypeRegistry.RegisterCommandParametersFactory(Instance);
    }
}
