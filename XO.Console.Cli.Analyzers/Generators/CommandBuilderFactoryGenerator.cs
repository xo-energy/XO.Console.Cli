using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class CommandBuilderFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find all uses of CommandBranchAttribute
        var commandBranchAttributes = InitializeCommandBranchAttributeProvider(context).Collect();

        // find all class declarations that implement ICommand<TParameters>
        var commands = InitializeCommandProvider(context);
        var commandsSource = commands
            .Select(static (command, _) =>
            {
                var source = new StringBuilder();
                EmitCommandFactoryCase(source, command.FullName, command.ParametersType!);
                return source.ToString();
            })
            .Collect();

        // combine command and branch models into the list of declarative commands
        var declarativeCommands = commands.Where(x => x.HasDeclarativeConfiguration).Collect();
        var declarativeCommandsByPath = InitializeDeclarativeCommandsByPath(declarativeCommands, commandBranchAttributes);

        // generate diagnostics
        context.RegisterSourceOutput(
            commands,
            static (context, command) =>
            {
                foreach (var diagnostic in command.Diagnostics)
                    context.ReportDiagnostic(diagnostic.CreateDiagnostic());
            });
        context.RegisterSourceOutput(
            commandBranchAttributes,
            static (context, commands) =>
            {
                foreach (var command in commands)
                {
                    foreach (var diagnostic in command.Diagnostics)
                        context.ReportDiagnostic(diagnostic.CreateDiagnostic());
                }
            });

        // generate implementation source
        context.RegisterImplementationSourceOutput(
            declarativeCommandsByPath.Combine(commandsSource),
            static (context, x) => Execute(context, x.Left, x.Right));
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<CommandModel> declarativeCommands,
        ImmutableArray<string> commandBuilderSource)
    {
        // don't output empty factories
        if (declarativeCommands.IsDefaultOrEmpty && commandBuilderSource.IsDefaultOrEmpty)
            return;

        var builder = new StringBuilder();

        builder.AppendLine(
            $$"""
            // <auto-generated/>
            using XO.Console.Cli.Infrastructure;

            #nullable enable

            namespace {{ThisAssembly.RootNamespace}}.Generated;

            /// <summary>
            /// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
            /// </summary>
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{ThisAssembly.AssemblyName}}", "{{ThisAssembly.AssemblyVersion}}")]
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
            internal sealed class CommandBuilderFactory : ICommandBuilderFactory
            {
            #pragma warning disable CS0436 // Type conflicts with imported type
                public static readonly CommandBuilderFactory Instance = new CommandBuilderFactory();
            #pragma warning restore CS0436 // Type conflicts with imported type

            """);

        builder.AppendLine(
            $$"""
                /// <inheritdoc/>
                public void AddCommandAttributeCommands(CommandBuilder builder)
                {
            """);

        // generate automatic configuration for CommandAttribute usage
        EmitConfigureCommandAppStatements(context, builder, declarativeCommands);

        builder.AppendLine(
            $$"""
                }

                /// <inheritdoc/>
                public CommandBuilder? CreateCommandBuilder<TCommand>(string verb)
                    where TCommand : class, ICommand
                {
            """);

        foreach (var source in commandBuilderSource)
        {
            builder.Append(source);
        }

        builder.AppendLine(
            $$"""
                    return null;
                }

                [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
                internal static void Register()
                {
                    TypeRegistry.RegisterCommandBuilderFactory(Instance);
                }
            }
            """);

        context.AddSource("CommandBuilderFactory.g.cs", builder.ToString());
    }
}
