using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XO.Console.Cli.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XO.Console.Cli.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CommandParametersFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var parametersDeclarations = InitializeParametersDeclarationProvider(context);

        var parametersSource = parametersDeclarations
            .Select(static (parameters, cancellationToken) =>
            {
                var source = new StringBuilder();

                EmitDescribeParametersCase(source, parameters);

                return source.ToString();
            });

        context.RegisterImplementationSourceOutput(
            parametersSource.Collect(),
            static (context, x) => Execute(context, x));

    }

    private static IncrementalValuesProvider<ParametersTypeModel> InitializeParametersDeclarationProvider(
        IncrementalGeneratorInitializationContext context)
    {
        var parametersDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax decl && decl.BaseList?.Types.Count > 0,
            static (context, cancellationToken) =>
            {
                var decl = (ClassDeclarationSyntax)context.Node;

                // get the declared type
                if (context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is not INamedTypeSymbol declType)
                    return null;

                // we only need to emit binders for types that can be instantiated
                if (declType.IsAbstract || declType.IsStatic)
                    return null;

                INamedTypeSymbol? currentType = null;
                var typesToBind = ImmutableStack<INamedTypeSymbol>.Empty;

                for (currentType = declType; currentType is not null; currentType = currentType.BaseType)
                {
                    if (currentType.EqualsSourceString("XO.Console.Cli.CommandParameters"))
                        break;

                    typesToBind = typesToBind.Push(currentType);
                }

                // we only care about types that inherit from CommandParameters
                if (currentType is null)
                    return null;

                var arguments = ImmutableList.CreateBuilder<CommandArgumentModel>();
                var options = ImmutableList.CreateBuilder<CommandOptionModel>();

                while (!typesToBind.IsEmpty)
                {
                    typesToBind = typesToBind.Pop(out var type);

                    foreach (var member in type.GetMembers())
                    {
                        if (member.Kind != SymbolKind.Property ||
                            member.IsStatic)
                            continue;

                        var property = (IPropertySymbol)member;

                        foreach (var attribute in property.GetAttributes())
                        {
                            switch (attribute.AttributeClass?.ToSourceString())
                            {
                                case "XO.Console.Cli.CommandArgumentAttribute"
                                when GetCommandArgumentAttributeData(property, attribute) is { } argument:
                                    arguments.Add(argument);
                                    break;

                                case "XO.Console.Cli.CommandOptionAttribute"
                                when GetCommandOptionAttributeData(property, attribute) is { } option:
                                    options.Add(option);
                                    break;
                            }
                        }
                    }
                }

                return new ParametersTypeModel(declType.ToSourceString(), arguments.ToImmutable(), options.ToImmutable());
            });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return parametersDeclarations.Where(x => x != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    private static CommandArgumentModel? GetCommandArgumentAttributeData(IPropertySymbol property, AttributeData attr)
    {
        if (attr.ConstructorArguments[1].IsNull)
            return null;

        var order = (int)attr.ConstructorArguments[0].Value!;
        var name = (string)attr.ConstructorArguments[1].Value!;
        var description = default(string?);
        var greedy = default(bool);
        var optional = default(bool);

        foreach (var entry in attr.NamedArguments)
        {
            switch (entry.Key)
            {
                case nameof(ICommandArgumentAttributeData.Description):
                    description = (string?)entry.Value.Value;
                    break;
                case nameof(ICommandArgumentAttributeData.IsGreedy):
                    greedy = (bool)entry.Value.Value!;
                    break;
                case nameof(ICommandArgumentAttributeData.IsOptional):
                    optional = (bool)entry.Value.Value!;
                    break;
            }
        }

        return new CommandArgumentModel(name, property, description)
        {
            Order = order,
            IsGreedy = greedy,
            IsOptional = optional,
        };
    }

    private static CommandOptionModel? GetCommandOptionAttributeData(IPropertySymbol property, AttributeData attr)
    {
        if (attr.ConstructorArguments[0].IsNull)
            return null;

        var name = (string)attr.ConstructorArguments[0].Value!;
        var aliases = Definitions.ConvertTypedConstantToImmutableArray<string>(attr.ConstructorArguments[1]);
        var description = default(string?);
        var hidden = default(bool);

        foreach (var entry in attr.NamedArguments)
        {
            switch (entry.Key)
            {
                case nameof(ICommandOptionAttributeData.Description):
                    description = (string?)entry.Value.Value;
                    break;
                case nameof(ICommandOptionAttributeData.IsHidden):
                    hidden = (bool)entry.Value.Value!;
                    break;
            }
        }

        return new CommandOptionModel(name, property, description, aliases)
        {
            IsHidden = hidden,
        };
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<string> parametersSource)
    {
        // don't output empty factories
        if (parametersSource.IsDefaultOrEmpty)
            return;

        var source = new StringBuilder();

        source.AppendLine(
            $$"""
            // <auto-generated/>
            using XO.Console.Cli.Infrastructure;
            using XO.Console.Cli.Model;

            #nullable enable

            namespace {{ThisAssembly.RootNamespace}}.Generated;

            /// <summary>
            /// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
            /// </summary>
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{ThisAssembly.AssemblyName}}", "{{ThisAssembly.AssemblyVersion}}")]
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
            internal sealed class CommandParametersFactory : ICommandParametersFactory
            {
            #pragma warning disable CS0436 // Type conflicts with imported type
                public static readonly CommandParametersFactory Instance = new CommandParametersFactory();
            #pragma warning restore CS0436 // Type conflicts with imported type

            """);

        source.AppendLine(
            $$"""
                public CommandParametersInfo? DescribeParameters(global::System.Type parametersType)
                {
                    if (parametersType is null)
                        throw new global::System.ArgumentNullException(nameof(parametersType));

            """);

        foreach (var item in parametersSource)
            source.Append(item);

        source.AppendLine(
            $$"""

                    return null;
                }

                [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
                internal static void Register()
                {
                    TypeRegistry.RegisterCommandParametersFactory(Instance);
                }
            }
            """);

        context.AddSource("CommandParametersFactory.g.cs", source.ToString());
    }

    private static void EmitDescribeParametersCase(StringBuilder source, ParametersTypeModel parametersModel)
    {
        source.AppendLine(
            $$"""
                    if (parametersType == typeof({{parametersModel.Name}}))
                    {
                        return new CommandParametersInfo(
            """);

        EmitDescribeParametersCaseArguments(source, parametersModel);
        EmitDescribeParametersCaseOptions(source, parametersModel);

        source.AppendLine(
            $$"""
                    }
            """);
    }

    private static void EmitDescribeParametersCaseArguments(StringBuilder source, ParametersTypeModel parametersModel)
    {
        var arguments = parametersModel.Arguments;

        if (arguments.Count == 0)
        {
            source.AppendLine(
            $$"""
                            global::System.Collections.Immutable.ImmutableArray<CommandArgument>.Empty,
            """);
        }
        else
        {
            source.AppendLine(
            $$"""
                            global::System.Collections.Immutable.ImmutableArray.Create<CommandArgument>(
            """);
        }

        for (int i = 0; i < arguments.Count; i++)
        {
            var model = arguments[i];
            source.AppendLine(
            $$"""
                                new CommandArgument(
                                    "{{model.Name}}",
                                    {{GetSetterLambdaExpression(model)}},
                                    typeof({{model.ParameterValueType}}),
                                    {{(model.Description is not null ? LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(model.Description)) : "null")}})
                                {
                                    Order = {{LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(model.Order))}},
                                    IsGreedy = {{LiteralExpression(model.IsGreedy ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)}},
                                    IsOptional = {{LiteralExpression(model.IsOptional ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)}},
                                }{{(i + 1 < arguments.Count ? "," : "),")}}
            """);
        }
    }

    private static void EmitDescribeParametersCaseOptions(StringBuilder source, ParametersTypeModel parametersModel)
    {
        var options = parametersModel.Options;

        if (options.Count == 0)
        {
            source.AppendLine(
            $$"""
                            global::System.Collections.Immutable.ImmutableArray<CommandOption>.Empty);
            """);
        }
        else
        {
            source.AppendLine(
            $$"""
                            global::System.Collections.Immutable.ImmutableArray.Create<CommandOption>(
            """);
        }

        for (int i = 0; i < options.Count; i++)
        {
            var model = options[i];
            var aliasLiteralParams = String.Join(", ", from alias in model.Aliases select LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(alias)));
            source.AppendLine(
            $$"""
                                new CommandOption(
                                    "{{model.Name}}",
                                    {{GetSetterLambdaExpression(model)}},
                                    typeof({{model.ParameterValueType}}),
                                    {{(model.Description is not null ? LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(model.Description)) : "null")}})
                                {
                                    Aliases = global::System.Collections.Immutable.ImmutableArray.Create<string>({{aliasLiteralParams}}),
                                    IsFlag = {{LiteralExpression(model.IsFlag ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)}},
                                    IsHidden = {{LiteralExpression(model.IsHidden ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)}},
                                }{{(i + 1 < options.Count ? "," : "));")}}
            """);
        }
    }

    private static string GetSetterLambdaExpression(CommandParameterModel parameter)
    {
        var defaultConverter = GetSetterDefaultConverter(parameter);

        switch (parameter.PropertyTypeKind)
        {
            case TypeKind.Array:
                return $$"""
                    static (context, values, converters) => (({{parameter.DeclaringType}})context.Parameters).{{parameter.PropertyName}} = ParameterValueConverter.ConvertArray<{{parameter.ParameterValueType}}>(values, converters, {{defaultConverter}})
                    """;

            default:
                return $$"""
                    static (context, values, converters) => (({{parameter.DeclaringType}})context.Parameters).{{parameter.PropertyName}} = ParameterValueConverter.ConvertSingle<{{parameter.ParameterValueType}}>(values, converters, {{defaultConverter}})
                    """;
        }
    }

    private static string GetSetterDefaultConverter(CommandParameterModel parameter)
    {
        switch (parameter.ParameterParsingStrategy)
        {
            case ParameterParsingStrategy.Constructor:
                return $$"""static (value) => new {{parameter.ParameterValueType}}(value)""";
            case ParameterParsingStrategy.Parse:
                return $$"""static (value) => {{parameter.ParameterValueType}}.Parse(value)""";
            case ParameterParsingStrategy.Enum:
            case ParameterParsingStrategy.None:
                return $$"""ParameterValueConverter.Parse{{parameter.ParameterParsingStrategy}}<{{parameter.ParameterValueType}}>""";
            case ParameterParsingStrategy.String:
                return $$"""static (value) => value""";
            default:
                return $$"""ParameterValueConverter.Parse{{parameter.ParameterParsingStrategy}}""";
        }
    }
}
