using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XO.Console.Cli.Generators;

[UsesVerify]
public sealed class CommandBuilderFactoryGeneratorTest
{
    [Fact]
    public Task GeneratesFactoryForCommandDeclaration()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithAttribute()
    {
        return VerifySource(
            """
            using System.ComponentModel;
            using XO.Console.Cli;

            namespace Test;

            [Command("command1", Aliases = new string[] { "command" }, IsHidden = false, Description = "Does something?")]
            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithBranchAttribute()
    {
        return VerifySource(
            """
            using System.ComponentModel;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(path) { }
            }

            [GroupCommand("command1", Aliases = new string[] { "command" }, IsHidden = false, Description = "Does something?")]
            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithBranchAttributeNested()
    {
        return VerifySource(
            """
            using System.ComponentModel;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(path) { }
            }

            [CommandBranch("group", "get")]
            internal sealed class GroupGetCommandAttribute : CommandAttribute
            {
                public GroupGetCommandAttribute(string verb)
                    : base(path) { }
            }

            [CommandBranch("group", "load")]
            internal sealed class GroupLoadCommandAttribute : CommandAttribute
            {
                public GroupLoadCommandAttribute(string verb)
                    : base(path) { }
            }

            [GroupLoadCommand("command1", Aliases = new string[] { "command" }, IsHidden = false, Description = "Does something?")]
            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }

            [GroupLoadCommand("command2", Description = "Does something else?")]
            public sealed class Command2 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }

            [GroupGetCommand("command")]
            public sealed class Command3 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithBranchAttributeOptions()
    {
        return VerifySource(
            """
            using System.ComponentModel;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group", Aliases = new string[] { "gromp" }, IsHidden = true, Description = "some commands", ParametersType = typeof(CommandParameters))]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(path) { }
            }

            [GroupCommand("command1", Aliases = new string[] { "command" }, IsHidden = true, Description = "Does something?")]
            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    private static Task VerifySource(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
            .Select(_ => MetadataReference.CreateFromFile(_.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(CommandParameters).Assembly.Location),
            })
            .ToArray();
        var compilation = CSharpCompilation.Create(
            assemblyName: "Test",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        var generator = new CommandBuilderFactoryGenerator();
        var generatorDriver = CSharpGeneratorDriver.Create(generator);

        var result = generatorDriver.RunGenerators(compilation);

        return Verify(result);
    }
}
