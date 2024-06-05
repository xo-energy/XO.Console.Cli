using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Generators;

public sealed partial class CommandBuilderFactoryGenerator
{
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
                var diagnostics = ImmutableList<Diagnostic>.Empty;
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

    private static IncrementalValuesProvider<CommandModel> InitializeCommandProvider(IncrementalGeneratorInitializationContext context)
    {
        var commandProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax decl && decl.BaseList?.Types.Count > 0,
            static (context, cancellationToken) =>
            {
                var decl = (ClassDeclarationSyntax)context.Node;
                if (context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is not INamedTypeSymbol declType)
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

                // we only care about types that implement ICommand
                if (commandParametersType is null)
                    return null;

                return GetCommandModel(decl, declType, commandParametersType.ToSourceString());
            });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return commandProvider.Where(x => x != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    private static IncrementalValueProvider<ImmutableArray<CommandModel>> InitializeDeclarativeCommandsByPath(
        IncrementalValueProvider<ImmutableArray<CommandModel>> declarativeCommands,
        IncrementalValueProvider<ImmutableArray<CommandModel>> commandBranchAttributes)
    {
        return IncrementalValueProviderExtensions.Combine(declarativeCommands, commandBranchAttributes)
            .Select(static (x, _) =>
            {
                var builder = x.Left.ToBuilder();

                builder.AddRange(x.Right);

                // sort the commands by path, placing equal-path branches before commands
                builder.Sort(static (x, y) =>
                {
                    for (int i = 0; i < x.Path.Length && i < y.Path.Length; i++)
                    {
                        var result = StringComparer.Ordinal.Compare(x.Path[i], y.Path[i]);
                        if (result != 0) return result;
                    }

                    if (x.Path.Length == y.Path.Length)
                        return x.Kind.CompareTo(y.Kind);
                    else if (x.Path.Length < y.Path.Length)
                        return -1;
                    else
                        return 1;
                });

                return builder.ToImmutable();
            });
    }

    private static void CheckCommandAttributeConstructors(ref ImmutableList<Diagnostic> diagnostics, INamedTypeSymbol targetSymbol, ClassDeclarationSyntax targetNode)
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

    private static CommandModel GetCommandModel(
        ClassDeclarationSyntax decl,
        INamedTypeSymbol declSymbol,
        string parametersType)
    {
        var declIdentifierLocation = decl.Identifier.GetLocation();
        var diagnosticsBuilder = ImmutableList.CreateBuilder<Diagnostic>();

        CommandAttributeData? attributeData = null;

        // find any declarative configuration attribute(s) applied to the command class
        foreach (var attribute in declSymbol.GetAttributes())
        {
            var attributePath = GetCommandAttributePath(attribute);
            if (attributePath.IsDefault)
                continue;

            // report a diagnostic for applying two declarative configuration attributes to the same command
            if (attributeData is not null)
            {
                diagnosticsBuilder.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandMayNotHaveMultipleCommandAttributes,
                        declIdentifierLocation,
                        declSymbol.ToSourceString()));

                // in case of a conflict, prefer the first custom attribute
                continue;
            }

            attributeData = GetCommandAttributeData(
                CommandModelKind.Command,
                attribute,
                attributePath);
        }

        var model = CommandModel.FromAttributeData(
            CommandModelKind.Command,
            declSymbol.ToSourceString(),
            declIdentifierLocation,
            diagnosticsBuilder.ToImmutable(),
            attributeData,
            parametersType);

        return model;
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
            path = [.. path, verb];
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

    private static ImmutableArray<string> GetCommandAttributePath(AttributeData attribute)
    {
        if (attribute.AttributeClass?.EqualsSourceString("XO.Console.Cli.CommandAttribute") == true)
            return ImmutableArray<string>.Empty;

        if (attribute.AttributeClass is not null)
        {
            foreach (var attributeAttribute in attribute.AttributeClass.GetAttributes())
            {
                if (attributeAttribute.AttributeClass?.EqualsSourceString("XO.Console.Cli.CommandBranchAttribute") == true)
                    return Definitions.ConvertTypedConstantToImmutableArray<string>(attributeAttribute.ConstructorArguments[0]);
            }
        }

        return default;
    }
}
