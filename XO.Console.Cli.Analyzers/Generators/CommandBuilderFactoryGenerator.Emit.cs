using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using XO.Console.Cli.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XO.Console.Cli.Generators;

public sealed partial class CommandBuilderFactoryGenerator
{
    private static void EmitConfigureCommandAppStatements(SourceProductionContext context, StringBuilder builder, ImmutableArray<CommandModel> commands)
    {
        var maxPathLength = 1;

        // declare a child command list for each non-leaf path depth
        foreach (var command in commands)
        {
            while (maxPathLength < command.Path.Length)
            {
                builder.Append(' ', 8);
                builder.AppendLine($"var commands{maxPathLength++} = global::System.Collections.Immutable.ImmutableList.CreateBuilder<ConfiguredCommand>();");
            }
        }

        // spacing
        if (maxPathLength > 1) builder.AppendLine();

        CommandModel? currentCommand = null;
        var commandStack = new Stack<(ImmutableArray<string> Path, CommandModel? Model)>();

        for (int i = 0; i < commands.Length; i++)
        {
            var candidate = commands[i];
            var candidatePath = candidate.Path;

            // check for duplicate paths or verbs
            switch (candidate.Kind)
            {
                case CommandModelKind.Branch when currentCommand != null:
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicatePathWillBeIgnored,
                            candidate.Location,
                            candidate.FullName,
                            String.Join(" ", candidate.Path),
                            currentCommand.FullName));
                    continue;

                case CommandModelKind.Command when currentCommand?.Kind == CommandModelKind.Command:
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateVerbWillBeIgnored,
                            candidate.Location,
                            candidate.FullName,
                            candidate.Verb,
                            currentCommand.FullName));
                    continue;
            }

            // push any missing path steps onto the stack
            for (var depth = commandStack.Count + 1; depth < candidatePath.Length; depth++)
                PushCommandStack(candidatePath[..depth], null);

            // decide what to do based on the next command's path
            var nextPath = i + 1 < commands.Length ? commands[i + 1].Path : [];
            var nextPathMatchLength = ImmutableArrayEqualityComparer.GetMatchLength(candidatePath, nextPath);
            if (nextPathMatchLength < candidatePath.Length)
            {
                currentCommand = null;

                // this is the last command in the subtree (it's a leaf), so emit it
                EmitConfiguredCommandStatement(builder, candidatePath, candidate, children: false);

                // do we need to pop any intermediate commands off the stack?
                while (commandStack.Count > nextPathMatchLength)
                {
                    var parent = commandStack.Pop();
                    EmitConfiguredCommandStatement(builder, parent.Path, parent.Model, children: true);
                }
            }
            else if (candidatePath.Length == nextPath.Length)
            {
                // defer handling this command because the next one has the same path
                currentCommand = candidate;
            }
            else
            {
                currentCommand = null;

                // the next command is a child of this one, so push this one onto the stack
                PushCommandStack(candidatePath, candidate);
            }
        }

        void PushCommandStack(ImmutableArray<string> path, CommandModel? command)
        {
            commandStack.Push((path, command));

            // after pushing, there aren't any children yet
            var depth = path.Length;
            if (depth < maxPathLength)
            {
                builder.Append(' ', 8);
                builder.AppendLine($"commands{depth}.Clear();");
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
