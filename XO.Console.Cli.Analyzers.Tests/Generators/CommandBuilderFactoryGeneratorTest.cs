using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Generators;

public sealed class CommandBuilderFactoryGeneratorTest
{
    [Fact]
    public void DoesNotGenerateEmptyFactory()
    {
        var driver = CompilationHelper.RunGenerator<CommandBuilderFactoryGenerator>(
            """
            namespace Test;

            public sealed class NotACommand
            {
                public void Execute() { }
            }
            """,
            out var outputCompilation,
            out _);

        Assert.Single(outputCompilation.SyntaxTrees);
    }

    [Fact]
    public void DoesNotReportDiagnostic_ForCommandAttributeWithPrivateConstructorAndPublicConstructor()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void DoesNotReportDiagnostic_ForCommandAttributeWithMultipleConstructors()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        Assert.Empty(diagnostics);
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

            [System.ComponentModel.Description("Hello")]
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
    public Task ReportsDiagnostic_ForCommandAttributeWithPrivateConstructor()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForCommandAttributeWithWrongConstructorParameters()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForCommandAttributeWithSomeWrongConstructorParameters()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForDuplicatePath()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForDuplicateVerb()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForMultipleCommandAttributes()
    {
        var diagnostics = GetGeneratorDiagnostics(
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

        return Verify(diagnostics);
    }

    [Fact]
    public Task ReportsDiagnostic_ForWrongCommandBranchAttributeTarget()
    {
        var diagnostics = GetGeneratorDiagnostics(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("group")]
            internal sealed class MyClass
            {
                public string Value { get; set; }
            }
            """);

        return Verify(diagnostics);
    }

    private static ImmutableArray<Diagnostic> GetGeneratorDiagnostics(string source)
    {
        _ = CompilationHelper.RunGenerator<CommandBuilderFactoryGenerator>(source, out _, out var diagnostics);
        return diagnostics;
    }

    private static async Task VerifySource(string source)
    {
        var driver = CompilationHelper.RunGeneratorAndAssertEmptyDiagnostics<CommandBuilderFactoryGenerator>(source);

        await Verify(driver)
            .ScrubGeneratedCodeAttribute()
            ;
    }
}
