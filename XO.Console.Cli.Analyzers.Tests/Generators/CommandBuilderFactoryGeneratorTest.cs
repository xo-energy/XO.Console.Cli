using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Generators;

public sealed class CommandBuilderFactoryGeneratorTest
{
    [Fact]
    public void DoesNotGenerateEmptyFactory()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            namespace Test;

            public sealed class NotACommand
            {
                public void Execute() { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void DoesNotReportDiagnostic_ForCommandAttributeWithPrivateConstructorAndPublicConstructor()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                private GroupCommandAttribute(string honk, int other)
                    : base(honk) { }

                public GroupCommandAttribute(string verb)
                    : this(verb, 1) { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void DoesNotReportDiagnostic_ForCommandAttributeWithMultipleConstructors()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb, int other)
                    : base(verb) { }

                public GroupCommandAttribute(string verb)
                    : this(verb, 1) { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public Task GeneratesFactoryForCommandBranchDeclaration()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration()
    {
        return VerifySource(
            """
            using System;
            using System.Threading;
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
            using System;
            using System.Threading;
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
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
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
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }

            [CommandBranch("group", "get")]
            internal sealed class GroupGetCommandAttribute : CommandAttribute
            {
                public GroupGetCommandAttribute(string verb)
                    : base(verb) { }
            }

            [CommandBranch("group", "load")]
            internal sealed class GroupLoadCommandAttribute : CommandAttribute
            {
                public GroupLoadCommandAttribute(string verb)
                    : base(verb) { }
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
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group", Aliases = new string[] { "gromp" }, IsHidden = true, Description = "some commands", ParametersType = typeof(CommandParameters))]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
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

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithMissingIntermediateBranch()
    {
        return VerifySource(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("get", "widgets")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }

            [GroupCommand("command1")]
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
    public Task GeneratesFactoryForCommandDeclaration_WithMultipleInterfaces()
    {
        // COVERAGE NOTE:
        //  This test doesn't hit MoveNext on the declaration's interface list because the base class implements
        //  ICommand<TParameters>, so that interface will always be first in the list. Is it possible to arrange things
        //  so it does?

        return VerifySource(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using XO.Console.Cli;

            namespace Test;

            public sealed class Command1 : AsyncCommand<CommandParameters>, ICloneable
            {
                public override Task<int> ExecuteAsync(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }

                public object Clone()
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithNullVerb()
    {
        // test the null attribute argument doesn't crash the generator, even though it will fail at runtime
        return VerifySource(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [Command(null)]
            internal sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
    }

    [Fact]
    public Task GeneratesFactoryForCommandDeclaration_WithUnrelatedAttribute()
    {
        return VerifySource(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [Obsolete]
            [Command("command1")]
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
    public void ReportsDiagnostic_ForCommandAttributeWithPrivateConstructor()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                private GroupCommandAttribute(string verb)
                    : base(verb) { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.CommandAttributeMustHavePublicConstructor.Id
                && diagnostic.GetMessage().Contains("Test.GroupCommandAttribute");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForCommandAttributeWithWrongConstructorParameters()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string honk)
                    : base(honk) { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.CommandAttributeConstructorsMustHaveVerbParameter.Id
                && diagnostic.GetMessage().Contains("Test.GroupCommandAttribute");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForCommandAttributeWithSomeWrongConstructorParameters()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute()
                    : base("") { }

                public GroupCommandAttribute(string verb, int other)
                    : base(verb) { }

                public GroupCommandAttribute(int verb)
                    : base("") { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.CommandAttributeConstructorsMustHaveVerbParameter.Id
                && diagnostic.GetMessage().Contains("Test.GroupCommandAttribute");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForDuplicatePath()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }

            [CommandBranch("group")]
            internal sealed class DuplicateCommandAttribute : CommandAttribute
            {
                public DuplicateCommandAttribute(string verb)
                    : base(verb) { }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.DuplicatePathWillBeIgnored.Id
                && diagnostic.GetMessage().Contains("Test.DuplicateCommandAttribute");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForDuplicateVerb()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [Command("command1")]
            internal sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }

            [Command("command1")]
            internal sealed class DuplicateCommand1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.DuplicateVerbWillBeIgnored.Id
                && diagnostic.GetMessage().Contains("Test.DuplicateCommand1");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForMultipleCommandAttributes()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }

            [CommandBranch("group", "sub")]
            internal sealed class SubCommandAttribute : CommandAttribute
            {
                public SubCommandAttribute(string verb)
                    : base(verb) { }
            }

            [GroupCommand("command1")]
            [SubCommand("command1")]
            internal sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.CommandMayNotHaveMultipleCommandAttributes.Id
                && diagnostic.GetMessage().Contains("Test.Command1");
        });
    }

    [Fact]
    public void ReportsDiagnostic_ForWrongCommandBranchAttributeTarget()
    {
        var driver = RunCommandBuilderFactoryGenerator(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class MyClass
            {
                public string Value { get; set; }
            }
            """);
        var result = driver.GetRunResult();

        Assert.Contains(result.Diagnostics, static (diagnostic) =>
        {
            return diagnostic.Id == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute.Id
                && diagnostic.GetMessage().Contains("Test.MyClass");
        });
    }

    private static GeneratorDriver RunCommandBuilderFactoryGenerator(string source)
    {
        const string main =
            """

            internal static class Program
            {
                public static int Main(string[] args) => 0;
            }
            """;

        var generator = new CommandBuilderFactoryGenerator();
        var compilation = CompilationHelper.CreateCompilation(source + main);
        var driver = CompilationHelper.RunGenerators(compilation, generator);

        // make sure there were no compilation errors (are we testing what we think we are?)
        Assert.Empty(compilation.GetDiagnostics());

        return driver;
    }

    private static Task VerifySource(string source)
    {
        var driver = RunCommandBuilderFactoryGenerator(source);

        return Verify(driver);
    }
}
