﻿//HintName: CommandParametersFactory.g.cs
// <auto-generated/>
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;

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

    public CommandParametersInfo? DescribeParameters(global::System.Type parametersType)
    {
        if (parametersType is null)
            throw new global::System.ArgumentNullException(nameof(parametersType));

        if (parametersType == typeof(Test.Parameters))
        {
            return new CommandParametersInfo(
                global::System.Collections.Immutable.ImmutableArray.Create<CommandArgument>(
                    new CommandArgument(
                        "name",
                        static (context, values, converters) => ((Test.Parameters)context.Parameters).Name = ParameterValueConverter.ConvertSingle<System.String>(values, converters, static (value) => value),
                        typeof(System.String),
                        null)
                    {
                        Order = 1,
                        IsGreedy = false,
                        IsOptional = false,
                    }),
                global::System.Collections.Immutable.ImmutableArray.Create<CommandOption>(
                    new CommandOption(
                        "--option",
                        static (context, values, converters) => ((Test.BaseParameters)context.Parameters).Enable = ParameterValueConverter.ConvertSingle<System.Boolean>(values, converters, static (value) => System.Boolean.Parse(value)),
                        typeof(System.Boolean),
                        null)
                    {
                        Aliases = global::System.Collections.Immutable.ImmutableArray.Create<string>("-o"),
                        IsFlag = true,
                        IsHidden = false,
                    }));
        }

        return null;
    }

    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Register()
    {
        TypeRegistry.RegisterCommandParametersFactory(Instance);
    }
}
