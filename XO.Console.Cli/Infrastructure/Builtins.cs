using System.Collections;
using System.Collections.Immutable;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Infrastructure;

internal static class Builtins
{
    public const string RemainingArgumentName = "__REMAINING__";

    public static class Arguments
    {
        public static CommandArgument Remaining { get; }
            = new CommandArgument(
                typeof(CommandParameters),
                nameof(Remaining),
                RemainingArgumentName,
                (context, values, _) => context.RemainingArguments.AddRange(values),
                typeof(string),
                description: "Captures remaining, unbound arguments")
            {
                IsGreedy = true,
                IsOptional = true,
            };
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
                typeof(CommandParameters),
                nameof(CliExplain),
                optionStyle.GetNameWithLeader("cli-explain"),
                DiscardValue,
                typeof(bool),
                description: "Explains the parser's interpretation of this command")
            {
                Aliases = ImmutableArray<string>.Empty,
                IsFlag = true,
                IsHidden = true,
            };
            Help = new CommandOption(
                typeof(CommandParameters),
                nameof(Help),
                optionStyle.GetNameWithLeader("help"),
                DiscardValue,
                typeof(bool),
                description: "Shows this help")
            {
                Aliases = ImmutableArray.Create(helpAlias),
                IsFlag = true,
            };
            Version = new CommandOption(
                typeof(CommandParameters),
                nameof(Version),
                optionStyle.GetNameWithLeader("version"),
                DiscardValue,
                typeof(bool),
                description: "Shows the application version")
            {
                Aliases = ImmutableArray<string>.Empty,
                IsFlag = true,
            };
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

        public static readonly CommandParameterSetter DiscardValue
            = static (_, _, _) => { /* pass */ };
    }
}
