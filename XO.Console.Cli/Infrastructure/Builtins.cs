using System.Collections;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Infrastructure;

internal static class Builtins
{
    public const string RemainingArgumentName = "__REMAINING__";

    public static class Arguments
    {
        public static CommandArgument Remaining { get; }
            = new CommandArgument(
                new CommandArgumentAttribute(int.MaxValue, RemainingArgumentName)
                { IsGreedy = true, IsOptional = true },
                typeof(Builtins),
                typeof(string[]),
                static (context, value) =>
                {
                    if (value != null)
                        context.RemainingArguments.AddRange((string[])value);
                },
                description: "Captures remaining, unbound arguments");
    }

    public static class Delegates
    {
        public static Task<int> NoOp<TParameters>(
            ICommandContext context,
            TParameters parameters,
            CancellationToken cancellationToken)
            => Task.FromResult(0);
    }

    public sealed class Options : IEnumerable<CommandOption>
    {
        public Options(CommandOptionStyle optionStyle)
        {
            var helpAlias = optionStyle switch
            {
                CommandOptionStyle.Dos => optionStyle.GetNameWithLeader("?"),
                _ => optionStyle.GetNameWithLeader("h"),
            };

            CliExplain = new CommandOption(
                new CommandOptionAttribute(optionStyle.GetNameWithLeader("cli-explain")) { IsHidden = true },
                typeof(Builtins),
                typeof(bool),
                DiscardValue,
                description: "Explains the parser's interpretation of this command");
            Help = new CommandOption(
                new CommandOptionAttribute(optionStyle.GetNameWithLeader("help"), helpAlias),
                typeof(Builtins),
                typeof(bool),
                DiscardValue,
                description: "Shows this help");
            Version = new CommandOption(
                new CommandOptionAttribute(optionStyle.GetNameWithLeader("version")),
                typeof(Builtins),
                typeof(bool),
                DiscardValue,
                description: "Shows the application version");
        }

        public CommandOption CliExplain { get; }
        public CommandOption Help { get; }
        public CommandOption Version { get; }

        public IEnumerator<CommandOption> GetEnumerator()
        {
            yield return CliExplain;
            yield return Help;
            yield return Version;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static void DiscardValue(CommandContext parameters, object? value)
        {
            // pass
        }
    }
}
