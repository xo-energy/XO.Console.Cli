using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XO.Console.Cli.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XO.Console.Cli.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CommandBuilderFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find all uses of CommandBranchAttribute
        var commandBranchAttributes = InitializeCommandBranchAttributeProvider(context);

        // find all class declarations that implement ICommand<TParameters>
        var commandDeclarations = InitializeCommandDeclarationProvider(context);

        // combine the two into the collection of declaratively-configured commands
        var commands = InitializeCommandProvider(
            context,
            commandDeclarations,
            commandBranchAttributes)
            .Where(static (x) => x != null);

        // re-combine the commands with the collection of branch attributes; both are required to generate the complete declarative command tree
        var commandsAndBranchAttributes = IncrementalValueProviderExtensions.Combine(
            commands.Collect(),
            commandBranchAttributes.Collect());

        // generate implementation source
        context.RegisterImplementationSourceOutput(
            commandsAndBranchAttributes,
            static (context, source) => Execute(context, source.Left.AddRange(source.Right)));
    }

    private static IncrementalValuesProvider<CommandModel> InitializeCommandBranchAttributeProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
            "XO.Console.Cli.CommandBranchAttribute",
            static (node, _) =>
            {
                // this should be validated by the compiler via AttributeTargets, so we don't need to return a diagnostic
                return node.IsKind(SyntaxKind.ClassDeclaration);
            },
            static (context, cancellationToken) =>
            {
                var attribute = context.Attributes[0];
                var diagnostics = ImmutableArray<Diagnostic>.Empty;
                var targetNode = (ClassDeclarationSyntax)context.TargetNode;
                var targetNodeIdentifierLocation = targetNode.Identifier.GetLocation();
                var targetSymbol = (INamedTypeSymbol)context.TargetSymbol;

                // validate that the target class inherits from CommandAttribute
                INamedTypeSymbol? baseType;
                for (baseType = targetSymbol.BaseType; baseType is not null; baseType = baseType.BaseType)
                {
                    if (baseType.EqualsSourceString("XO.Console.Cli.CommandAttribute"))
                        break;
                }

                // generate diagnostic if the above check failed
                if (baseType is null)
                {
                    diagnostics = diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute,
                            targetNodeIdentifierLocation,
                            targetSymbol.ToDisplayString()));
                }
                else
                {
                    // generate diagnostic if the target class does not have a 'verb' constructor argument
                    CheckCommandAttributeConstructors(ref diagnostics, targetSymbol, targetNode);
                }

                var attributeData = GetCommandAttributeData(
                    CommandModelKind.Branch,
                    attribute,
                    ImmutableArray<string>.Empty);

                return CommandModel.FromAttributeData(
                    CommandModelKind.Branch,
                    targetSymbol.ToSourceString(),
                    targetNodeIdentifierLocation,
                    diagnostics,
                    attributeData);
            })
            ;
    }

    private static IncrementalValuesProvider<CommandDeclaration> InitializeCommandDeclarationProvider(IncrementalGeneratorInitializationContext context)
    {
        var commandDeclarationSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax decl && decl.BaseList?.Types.Count > 0,
            static (context, _) => (ClassDeclarationSyntax)context.Node);

        var commandDeclarations = commandDeclarationSyntaxProvider.Combine(context.CompilationProvider)
            .Select(static (x, cancellationToken) =>
            {
                var (decl, compilation) = x;

                var semanticModel = compilation.GetSemanticModel(decl.SyntaxTree);

                // get the declared type
                if (semanticModel.GetDeclaredSymbol(decl, cancellationToken) is not INamedTypeSymbol declType)
                    return null;

                // we don't care about abstract or static types
                if (declType.IsAbstract || declType.IsStatic)
                    return null;

                ITypeSymbol? commandParametersType = null;

                // get any implementation of ICommand<TParameters>
                foreach (var @interface in declType.AllInterfaces)
                {
                    if (@interface.ConstructedFrom?.EqualsSourceString("XO.Console.Cli.ICommand<TParameters>") == true)
                    {
                        commandParametersType = @interface.TypeArguments[0];
                        break;
                    }
                }

                // we don't care about types that don't implement ICommand
                if (commandParametersType is null)
                    return null;

                return new CommandDeclaration(decl, commandParametersType.ToSourceString());
            });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return commandDeclarations.Where(x => x != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    private static IncrementalValuesProvider<CommandModel> InitializeCommandProvider(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<CommandDeclaration> commandDeclarations,
        IncrementalValuesProvider<CommandModel> commandBranchAttributes)
    {
        var commandBranchAttributeIndex = commandBranchAttributes
            .Collect()
            .Select(static (commandBranchAttributes, _) =>
            {
                var commandBranchAttributeIndex = new Dictionary<string, CommandModel>(commandBranchAttributes.Length);

                // index non-broken branch attributes for reference by command declarations
                foreach (var commandBranchAttribute in commandBranchAttributes)
                {
                    if (commandBranchAttribute.Diagnostics.IsDefaultOrEmpty)
                        commandBranchAttributeIndex.Add(commandBranchAttribute.FullName, commandBranchAttribute);
                }

                return commandBranchAttributeIndex;
            });

        var commandDeclarationsAndAttributes = IncrementalValueProviderExtensions.Combine(
            commandDeclarations,
            commandBranchAttributeIndex);

        var commandModels = commandDeclarationsAndAttributes.Combine(context.CompilationProvider)
            .Select(static (x, cancellationToken) =>
            {
                var (_, compilation) = x;
                var (commandDeclaration, commandBranchAttributeIndex) = x.Left;

                var location = commandDeclaration.SyntaxNode.Identifier.GetLocation();
                var diagnostics = ImmutableArray<Diagnostic>.Empty;

                var semanticModel = compilation.GetSemanticModel(commandDeclaration.SyntaxNode.SyntaxTree);

                // verify the declaration still has the properties we detected earlier
                if (semanticModel.GetDeclaredSymbol(commandDeclaration.SyntaxNode, cancellationToken)
                    is not INamedTypeSymbol
                    {
                        IsAbstract: false,
                        IsStatic: false,
                        TypeKind: TypeKind.Class,
                    }
                    declSymbol)
                    return null;

                CommandAttributeData? attributeData = null;
                var attributeDataPriority = 0;

                // find any declarative configuration attribute(s) applied to the command class
                foreach (var attribute in declSymbol.GetAttributes())
                {
                    var attributeClassName = attribute.AttributeClass!.ToSourceString();
                    var attributeDataCandidate = default(CommandAttributeData?);
                    int attributeDataCandidatePriority;

                    if (attributeClassName == "XO.Console.Cli.CommandAttribute")
                    {
                        attributeDataCandidate = GetCommandAttributeData(
                            CommandModelKind.Command,
                            attribute,
                            ImmutableArray<string>.Empty);
                        attributeDataCandidatePriority = 1;
                    }
                    else if (commandBranchAttributeIndex.TryGetValue(attributeClassName, out var branchAttribute))
                    {
                        attributeDataCandidate = GetCommandAttributeData(
                            CommandModelKind.Command,
                            attribute,
                            branchAttribute.Path);
                        attributeDataCandidatePriority = 2;
                    }
                    else
                    {
                        continue;
                    }

                    // report a diagnostic for applying two declarative configuration attributes to the same command
                    if (attributeData is not null)
                    {
                        diagnostics = diagnostics.Add(
                            Diagnostic.Create(
                                DiagnosticDescriptors.CommandMayNotHaveMultipleCommandAttributes,
                                location,
                                declSymbol.ToSourceString()));
                    }

                    // in case of a conflict, prefer the first custom attribute
                    if (attributeDataPriority < attributeDataCandidatePriority)
                    {
                        attributeData = attributeDataCandidate;
                        attributeDataPriority = attributeDataCandidatePriority;
                    }
                }

                return CommandModel.FromAttributeData(
                    CommandModelKind.Command,
                    declSymbol.ToSourceString(),
                    location,
                    diagnostics,
                    attributeData,
                    commandDeclaration.ParametersType);
            });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return commandModels.Where(x => x != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    private static void CheckCommandAttributeConstructors(ref ImmutableArray<Diagnostic> diagnostics, INamedTypeSymbol targetSymbol, ClassDeclarationSyntax targetNode)
    {
        var targetSymbolIdentifierLocation = targetNode.Identifier.GetLocation();
        var targetSymbolPublicConstructorsCount = 0;

        foreach (var constructor in targetSymbol.InstanceConstructors)
        {
            if (constructor.DeclaredAccessibility != Accessibility.Public || constructor.IsImplicitlyDeclared)
                continue;

            if (constructor.Parameters.Length < 1 ||
                constructor.Parameters[0].Type.SpecialType != SpecialType.System_String ||
                !constructor.Parameters[0].Name.Equals("verb", StringComparison.OrdinalIgnoreCase))
            {
                var location = targetSymbolIdentifierLocation;

                // place the squiggle on the constructor identifier if it's available
                if (constructor.DeclaringSyntaxReferences[0].GetSyntax() is ConstructorDeclarationSyntax constructorNode)
                    location = constructorNode.Identifier.GetLocation();

                diagnostics = diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandAttributeConstructorsMustHaveVerbParameter,
                        location,
                        constructor.ToDisplayString()));
            }

            targetSymbolPublicConstructorsCount++;
        }

        if (targetSymbolPublicConstructorsCount == 0)
        {
            diagnostics = diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.CommandAttributeMustHavePublicConstructor,
                    targetSymbolIdentifierLocation,
                    targetSymbol.ToDisplayString()));
        }
    }

    private static CommandAttributeData GetCommandAttributeData(
        CommandModelKind kind,
        AttributeData attribute,
        ImmutableArray<string> path)
    {
        if (kind == CommandModelKind.Branch)
        {
            path = Definitions.ConvertTypedConstantToImmutableArray<string>(attribute.ConstructorArguments[0]);
        }
        else if (
            kind == CommandModelKind.Command &&
            attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive &&
            attribute.ConstructorArguments[0].Value is string verb)
        {
            path = path.Add(verb);
        }
        else
        {
            path = ImmutableArray<string>.Empty;
        }

        var aliases = ImmutableArray<string>.Empty;
        var description = default(string?);
        var hidden = default(bool);
        var parametersType = default(string?);

        foreach (var entry in attribute.NamedArguments)
        {
            switch (entry.Key)
            {
                case "Aliases":
                    aliases = Definitions.ConvertTypedConstantToImmutableArray<string>(entry.Value);
                    break;
                case "Description":
                    description = (string?)entry.Value.Value;
                    break;
                case "IsHidden":
                    hidden = (bool)entry.Value.Value!;
                    break;
                case "ParametersType":
                    parametersType = ((INamedTypeSymbol)entry.Value.Value!).ToSourceString();
                    break;
            }
        }

        return new CommandAttributeData(
            path,
            aliases,
            description,
            hidden,
            parametersType);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<CommandModel> commands)
    {
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

        var commandsByPath = commands
            .Where(x => x.HasDeclarativeConfiguration)
            .ToLookup(x => x.Path, ImmutableArrayEqualityComparer<string>.Default);
        var commandsByPathIndex = new Dictionary<ImmutableArray<string>, CommandModel>(
            commandsByPath.Count,
            ImmutableArrayEqualityComparer<string>.Default);

        foreach (var group in commandsByPath)
        {
            CommandModel? branch = null;
            CommandModel? command = null;

            foreach (var candidate in group)
            {
                switch (candidate.Kind)
                {
                    case CommandModelKind.Branch when branch != null:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.DuplicatePathWillBeIgnored,
                                candidate.Location,
                                candidate.FullName,
                                String.Join(" ", candidate.Path),
                                branch.FullName));
                        break;

                    case CommandModelKind.Branch:
                        branch = candidate;
                        break;

                    case CommandModelKind.Command when command != null:
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateVerbWillBeIgnored,
                                candidate.Location,
                                candidate.FullName,
                                candidate.Verb,
                                command.FullName));
                        break;

                    case CommandModelKind.Command:
                        command = candidate;
                        break;
                }
            }

            // if we have a command, we don't need to generate anything for the branch
            commandsByPathIndex.Add(group.Key, command ?? branch!);
        }

        EmitConfigureCommandAppStatements(builder, commandsByPathIndex);

        builder.AppendLine(
            $$"""
                }

                /// <inheritdoc/>
                public CommandBuilder? CreateCommandBuilder<TCommand>(string verb)
                    where TCommand : class, ICommand
                {
            """);

        var commandCount = 0;

        foreach (var command in commands)
        {
            foreach (var diagnostic in command.Diagnostics)
                context.ReportDiagnostic(diagnostic);

            // it should be impossible for ParametersType to be null for CommandModelKind.Command
            if (command.Kind == CommandModelKind.Command)
            {
                EmitCommandFactoryCase(builder, command.FullName, command.ParametersType!);
                commandCount++;
            }
        }

        // don't output empty factories
        if (commandCount == 0 && commandsByPathIndex.Count == 0)
            return;

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

    private static void EmitConfigureCommandAppStatements(StringBuilder builder, IReadOnlyDictionary<ImmutableArray<string>, CommandModel> commandsByPathIndex)
    {
        var branchesByPath = commandsByPathIndex.Keys
            .SelectMany(
                static x => Enumerable.Range(0, x.Length),
                static (path, i) => new CommandBranch(Path: path[..i], Branch: path[..(i + 1)]))
            .Distinct()
            .ToLookup(x => x.Path, x => x.Branch, ImmutableArrayEqualityComparer<string>.Default);

        var pathStack = new Stack<ImmutableArray<string>>(
            branchesByPath[ImmutableArray<string>.Empty]);
        var depth = 1;
        var maxPathLength = 1;

        foreach (var path in commandsByPathIndex.Keys)
        {
            while (maxPathLength < path.Length)
            {
                builder.Append(' ', 8);
                builder.AppendLine($"var commands{maxPathLength++} = global::System.Collections.Immutable.ImmutableList.CreateBuilder<ConfiguredCommand>();");
            }
        }

        if (maxPathLength > 1)
        {
            builder.AppendLine();
        }

        while (pathStack.Count > 0)
        {
            var path = pathStack.Peek();

            // it's possible for no branch attribute to be created for an intermediate path
            if (!commandsByPathIndex.TryGetValue(path, out var command))
                command = null;

            if (depth > path.Length)
            {
                _ = pathStack.Pop();
                EmitConfiguredCommandStatement(builder, path, command, children: true);
                depth--;
                continue;
            }

            var children = 0;

            foreach (var next in branchesByPath[path])
            {
                pathStack.Push(next);
                children++;
            }

            // if there are no children, add this command to the current list
            if (children == 0)
            {
                _ = pathStack.Pop();
                EmitConfiguredCommandStatement(builder, path, command, children: false);
            }
            else
            {
                builder.Append(' ', 8);
                builder.AppendLine($"commands{depth}.Clear();");
                depth++;
            }
        }
    }

    private static void EmitConfiguredCommandStatement(
        StringBuilder builder,
        ImmutableArray<string> path,
        CommandModel? command,
        bool children)
    {
        var depth = path.Length;
        var indent = new string(' ', 8);
        var verb = command?.Verb ?? path[^1];
        var verbLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(verb));

        if (depth == 1)
        {
            builder.AppendLine($"{indent}builder.AddCommand(");
        }
        else
        {
            builder.AppendLine($"{indent}commands{depth - 1}.Add(");
        }

        indent += "    ";

        if (command?.Kind == CommandModelKind.Command)
        {
            builder.Append($"{indent}new ConfiguredCommand({verbLiteral}, static (resolver) => resolver.Get<{command.FullName}>(), typeof({command.ParametersType}))");
        }
        else if (command?.ParametersType != null)
        {
            builder.Append($"{indent}new ConfiguredCommand({verbLiteral}, static (_) => new MissingCommand(), typeof({command.ParametersType}))");
        }
        else
        {
            builder.Append($"{indent}new ConfiguredCommand({verbLiteral}, static (_) => new MissingCommand(), typeof(CommandParameters))");
        }

        if (command?.HasBuilderOptions == true || children)
        {
            builder.AppendLine();
            builder.AppendLine($"{indent}{{");

            if (command?.Aliases.Length > 0)
            {
                builder.Append($"{indent}    Aliases = global::System.Collections.Immutable.ImmutableHashSet.Create(");

                for (int i = 0; i < command.Aliases.Length; i++)
                {
                    if (i > 0) builder.Append(", ");
                    builder.Append(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(command.Aliases[i])));
                }

                builder.AppendLine("),");
            }

            if (children)
                builder.AppendLine($"{indent}    Commands = commands{depth}.ToImmutable(),");

            if (command?.Description is not null)
                builder.AppendLine($"{indent}    Description = {LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(command.Description))},");

            if (command?.IsHidden == true)
                builder.AppendLine($"{indent}    IsHidden = {LiteralExpression(SyntaxKind.TrueLiteralExpression)},");

            builder.Append($"{indent}}}");
        }

        builder.AppendLine(");");
    }

    private static void EmitCommandFactoryCase(StringBuilder builder, string fullName, string parametersType)
    {
        builder.AppendLine(
            $$"""
                    if (typeof(TCommand) == typeof({{fullName}}))
                    {
                        return new CommandBuilder(
                            verb,
                            static (resolver) => resolver.Get<{{fullName}}>(),
                            typeof({{parametersType}}));
                    }
            """);
    }
}
