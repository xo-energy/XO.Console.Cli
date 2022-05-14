using System;
using System.IO;
using System.Linq;
using System.Threading;
using XO.Console.Cli.Commands;
using Xunit;

namespace XO.Console.Cli.Tests;

public class CommandParametersBinderTest
{
    private readonly CommandParametersBinder DefaultBinder
        = new(CommandAppDefaults.Converters);

    [Fact]
    public void CanBindToConstructor()
    {
        var argument = "value";
        var expected = new TestRecord(argument);
        CanBindTo(expected, argument);
    }

    [Fact]
    public void CanBindToDirectoryInfo()
    {
        var path = Path.GetTempPath();
        var expected = new DirectoryInfo(path);
        CanBindTo(
            static (expected, actual) =>
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.FullName, actual!.FullName);
            },
            expected,
            path);
    }

    [Fact]
    public void CanBindToFileInfo()
    {
        var path = Path.Combine(Path.GetTempPath(), "test.txt");
        var expected = new FileInfo(path);
        CanBindTo(
            static (expected, actual) =>
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.FullName, actual!.FullName);
            },
            expected,
            path);
    }

    [Fact]
    public void CanBindToGuid()
    {
        var argument = "e8e313e7-55a6-4f07-8562-0c75e279f03a";
        var expected = Guid.Parse(argument);
        CanBindTo(expected, argument);
    }

    [Fact]
    public void CanBindToGuidArray()
    {
        var arguments = new[]
        {
            "e8e313e7-55a6-4f07-8562-0c75e279f03a",
            "89817ef9-f539-474d-bf65-3e2821ee1805",
        };
        var expected = arguments.Select(Guid.Parse).ToArray();
        CanBindTo(expected, arguments);
    }

    [Fact]
    public void CanBindToUri()
    {
        var argument = "http://www.google.com";
        var expected = new Uri(argument);
        CanBindTo(expected, argument);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(true, "True")]
    [InlineData(true, "TRUE")]
    [InlineData(false, "false")]
    [InlineData(false, "False")]
    [InlineData(false, "FALSE")]
    public void CanBindToBoolean(bool expected, string argument)
    {
        CanBindTo(expected, argument);
    }

    [Fact]
    public void CanBindToInt32()
    {
        CanBindTo(57, "57");
    }

    [Fact]
    public void CanBindToInt32Array()
    {
        CanBindTo(new[] { -5, 7 }, "-5", "7");
    }

    [Theory]
    [InlineData(-5, "-5")]
    [InlineData(7, "7")]
    public void CanBindToInt32Nullable(int? expected, string argument)
    {
        CanBindTo(expected, argument);
    }

    [Fact]
    public void CanBindToInt32NullableArray()
    {
        CanBindTo(new int?[] { -5, 7 }, "-5", "7");
    }

    [Fact]
    public void CanBindToString()
    {
        CanBindTo("value", "value");
    }

    [Fact]
    public void CanBindToStringArray()
    {
        CanBindTo(new[] { "value1", "value2" }, "value1", "value2");
    }

    private void CanBindTo<TValue>(TValue expected, params string[] arguments)
        => CanBindTo(Assert.Equal, expected, arguments);

    private void CanBindTo<TValue>(Action<TValue, TValue?> assert, TValue expected, params string[] arguments)
    {
        var context = new TestParameterContext<TValue>(isGreedy: true, isOptional: true);
        var tokens = new CommandToken[arguments.Length];

        for (int i = 0; i < arguments.Length; ++i)
            tokens[i] = new CommandToken(CommandTokenType.Argument, arguments[i], context.Argument);

        var bindings = DefaultBinder.BindParameters(tokens);

        foreach (var (parameter, value) in bindings)
            parameter.Setter(context.Context, value);

        assert(expected, context.Value);
    }

    private abstract class TestParameterContext
    {
        protected static int _order;
    }

    private sealed class TestParameterContext<TValue> : TestParameterContext
    {
        private static readonly MissingCommand Command = new MissingCommand();
        private static readonly CommandParameters Parameters = new CommandParameters();

        public TestParameterContext(bool isGreedy = false, bool isOptional = false)
        {
            var order = Interlocked.Increment(ref _order);
            this.Argument = new CommandArgument(
                new CommandArgumentAttribute(order, $"argument{order}")
                {
                    IsGreedy = isGreedy,
                    IsOptional = isOptional,
                },
                typeof(TestParameterContext<>).DeclaringType!,
                typeof(TValue),
                (_, value) => this.Value = (TValue?)value);
            this.Context = new CommandContext(Command, Parameters);
        }

        public CommandArgument Argument { get; }
        public CommandContext Context { get; }
        public TValue? Value { get; set; }
    }

    private sealed record TestRecord(string Value);
}
