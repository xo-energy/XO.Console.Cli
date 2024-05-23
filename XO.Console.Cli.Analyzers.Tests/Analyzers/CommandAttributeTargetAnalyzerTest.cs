using Microsoft.CodeAnalysis.Diagnostics;

namespace XO.Console.Cli.Analyzers;

public sealed class CommandAttributeTargetAnalyzerTest
{
    [Fact]
    public async Task DoesNotReportDiagnostic_ForOtherDeclaration()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            namespace Test;

            public enum MyEnum { Default }
            """);

        Assert.Empty(await compilation.GetAllDiagnosticsAsync());
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_ForValidCommand()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using System;
            using System.Threading;
            using XO.Console.Cli;

            namespace Test;

            [Command("go")]
            public sealed class Command1 : Command<CommandParameters>
            {
                public override int Execute(ICommandContext context, CommandParameters parameters, CancellationToken cancellationToken)
                {
                    throw new NotImplementedException();
                }
            }
            """);

        Assert.Empty(await compilation.GetAllDiagnosticsAsync());
    }

    [Fact]
    public async Task ReportsDiagnostic_ForAbstractClass()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using XO.Console.Cli;

            namespace Test;

            [Command("go")]
            public abstract class MyCommand : AsyncCommand<CommandParameters>
            {
            }
            """);

        Assert.Collection(
            await compilation.GetAllDiagnosticsAsync(),
            static (diagnostic) =>
            {
                Assert.Multiple(
                    () => Assert.Equal(DiagnosticDescriptors.CommandTypeMustNotBeAbstract.Id, diagnostic.Id),
                    () => Assert.Contains("Test.MyCommand", diagnostic.GetMessage()));
            });
    }

    [Fact]
    public async Task ReportsDiagnostic_ForAbstractClass_WithDerivedAttribute()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using XO.Console.Cli;

            namespace Test;

            [CommandBranch("go")]
            public class GroupCommandAttribute : CommandAttribute
            {
                public GroupCommandAttribute(string verb)
                    : base(verb) { }
            }

            [GroupCommand("now")]
            public abstract class MyCommand : AsyncCommand<CommandParameters>
            {
            }
            """);

        Assert.Collection(
            await compilation.GetAllDiagnosticsAsync(),
            static (diagnostic) =>
            {
                Assert.Multiple(
                    () => Assert.Equal(DiagnosticDescriptors.CommandTypeMustNotBeAbstract.Id, diagnostic.Id),
                    () => Assert.Contains("Test.MyCommand", diagnostic.GetMessage()));
            });
    }

    [Fact]
    public async Task ReportsDiagnostic_ForAbstractClass_WithOtherAttribute()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using System;
            using XO.Console.Cli;

            namespace Test;

            [Obsolete]
            [Command("go")]
            public abstract class MyCommand : AsyncCommand<CommandParameters>
            {
            }
            """);

        Assert.Collection(
            await compilation.GetAllDiagnosticsAsync(),
            static (diagnostic) =>
            {
                Assert.Multiple(
                    () => Assert.Equal(DiagnosticDescriptors.CommandTypeMustNotBeAbstract.Id, diagnostic.Id),
                    () => Assert.Contains("Test.MyCommand", diagnostic.GetMessage()));
            });
    }

    [Fact]
    public async Task ReportsDiagnostic_ForClassThatDoesNotImplementICommand()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using XO.Console.Cli;

            namespace Test;

            [Command("go")]
            public sealed class MyCommand
            {
            }
            """);

        Assert.Collection(
            await compilation.GetAllDiagnosticsAsync(),
            static (diagnostic) =>
            {
                Assert.Multiple(
                    () => Assert.Equal(DiagnosticDescriptors.CommandTypeMustImplementICommand.Id, diagnostic.Id),
                    () => Assert.Contains("Test.MyCommand", diagnostic.GetMessage()));
            });
    }

    [Fact]
    public async Task ReportsDiagnostic_ForClassThatDoesNotImplementICommand_WithOtherInterface()
    {
        var compilation = RunCommandAttributeTargetAnalyzer(
            """
            using System;
            using XO.Console.Cli;

            namespace Test;

            [Command("go")]
            public sealed class MyCommand : ICloneable
            {
                public object Clone() => throw new NotImplementedException();
            }
            """);

        Assert.Collection(
            await compilation.GetAllDiagnosticsAsync(),
            static (diagnostic) =>
            {
                Assert.Multiple(
                    () => Assert.Equal(DiagnosticDescriptors.CommandTypeMustImplementICommand.Id, diagnostic.Id),
                    () => Assert.Contains("Test.MyCommand", diagnostic.GetMessage()));
            });
    }

    private static CompilationWithAnalyzers RunCommandAttributeTargetAnalyzer(string source)
    {
        const string main =
            """

            internal static class Program
            {
                public static int Main(string[] args) => 0;
            }
            """;

        return CompilationHelper.CreateCompilation(source + main)
            .WithAnalyzers([new CommandAttributeTargetAnalyzer()]);
    }
}
