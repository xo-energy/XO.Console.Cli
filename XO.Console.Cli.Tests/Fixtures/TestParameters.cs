using System.ComponentModel.DataAnnotations;

namespace XO.Console.Cli.Tests.Fixtures;

public static class TestParameters
{
    public class Argument : CommandParameters
    {
        [CommandArgument(1, "arg")]
        public string? Arg { get; set; }
    }

    public class Greedy : CommandParameters
    {
        [CommandArgument(1, "args", IsGreedy = true)]
        public string[]? Args { get; set; }
    }

    public class GreedyOptional : CommandParameters
    {
        [CommandArgument(1, "args", IsGreedy = true, IsOptional = true)]
        public string[]? Args { get; set; }
    }

    public class Option : CommandParameters
    {
        [CommandOption("--option")]
        public string? Value { get; set; }
    }

    public class OptionFlag : CommandParameters
    {
        [CommandOption("--option")]
        public bool Value { get; set; }
    }

    public class OptionGroup : CommandParameters
    {
        [CommandOption("-a")]
        public bool ValueA { get; set; }

        [CommandOption("--bee", "-b")]
        public bool ValueB { get; set; }

        [CommandOption("-c")]
        public bool ValueC { get; set; }

        [CommandOption("--deer", "-d")]
        public bool ValueD { get; set; }
    }

    public class ValidationFailure : CommandParameters
    {
        public override ValidationResult Validate()
        {
            return new ValidationResult("Error!");
        }
    }
}
