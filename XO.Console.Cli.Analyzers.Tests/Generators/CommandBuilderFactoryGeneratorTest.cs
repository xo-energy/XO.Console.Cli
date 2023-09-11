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

            [Command("command1", Aliases = new string[] { "command" }, IsHidden = false), Description("Does something?")]
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
