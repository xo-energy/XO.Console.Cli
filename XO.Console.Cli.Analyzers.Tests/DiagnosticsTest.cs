using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XO.Console.Cli.Generators;

namespace XO.Console.Cli;

public sealed class DiagnosticsTest
{
    [Fact]
    public void CommandBranchAttributeMustBeAppliedToCommandAttribute_NotReportedForAttributeClass()
    {
        var driver = CompilationHelper.RunGenerators(Fixtures.MyAttribute, new CommandBuilderFactoryGenerator());
        var result = driver!.GetRunResult();

        Assert.DoesNotContain(result.Diagnostics, d => d.Descriptor == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute);
    }

    [Fact]
    public void CommandBranchAttributeMustBeAppliedToCommandAttribute_NotReportedForAttributeSubclass()
    {
        var driver = CompilationHelper.RunGenerators(Fixtures.MyAttributeSubclass, new CommandBuilderFactoryGenerator());
        var result = driver!.GetRunResult();

        Assert.DoesNotContain(result.Diagnostics, d => d.Descriptor == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute);
    }

    [Fact]
    public void CommandBranchAttributeMustBeAppliedToCommandAttribute_ReportedForClass()
    {
        var driver = CompilationHelper.RunGenerators(Fixtures.NotAnAttribute, new CommandBuilderFactoryGenerator());
        var result = driver!.GetRunResult();

        Assert.Contains(result.Diagnostics, d => d.Descriptor == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute);
    }

    [Fact]
    public void CommandBranchAttributeMustBeAppliedToCommandAttribute_ReportedForSubclass()
    {
        var driver = CompilationHelper.RunGenerators(Fixtures.NotAnAttributeSubclass, new CommandBuilderFactoryGenerator());
        var result = driver!.GetRunResult();

        Assert.Contains(result.Diagnostics, d => d.Descriptor == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute);
    }

    [Fact]
    public void CommandBranchAttributeMustBeAppliedToCommandAttribute_ReportedForWrongAttributeClass()
    {
        var driver = CompilationHelper.RunGenerators(Fixtures.NotAnAttributeSubclass, new CommandBuilderFactoryGenerator());
        var result = driver!.GetRunResult();

        Assert.Contains(result.Diagnostics, d => d.Descriptor == DiagnosticDescriptors.CommandBranchAttributeMustBeAppliedToCommandAttribute);
    }

    private static class Fixtures
    {
        public const string MyAttribute = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class MyAttribute : XO.Console.Cli.CommandAttribute
            {
                public MyAttribute(string verb) : base(verb) { }
            }
            """;

        public const string MyAttributeWrongConstructorParameterName = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class MyAttribute : XO.Console.Cli.CommandAttribute
            {
                public MyAttribute(string v) : base(v) { }
            }
            """;

        public const string MyAttributeWrongConstructorParameterType = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class MyAttribute : XO.Console.Cli.CommandAttribute
            {
                public MyAttribute(int count) : base("") { }
            }
            """;

        public const string MyAttributeSubclass = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class MyAttribute : MyBaseAttribute
            {
                public MyAttribute(string verb) : base(verb) { }
            }

            public abstract class MyBaseAttribute : XO.Console.Cli.CommandAttribute
            {
                public MyBaseAttribute(string verb) : base(verb) { }
            }
            """;

        public const string NotAnAttribute = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class NotAnAttribute { }
            """;

        public const string NotAnAttributeSubclass = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class NotAnAttribute : NotAnAttributeEither { }

            public abstract class NotAnAttributeEither { }
            """;

        public const string WrongAttribute = """
            [XO.Console.Cli.CommandBranch("test")]
            public sealed class MyAttribute : XO.Console.Cli.CommandArgumentAttribute { }
            """;
    }
}
