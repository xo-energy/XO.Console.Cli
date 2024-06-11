namespace XO.Console.Cli.Commands;

[CommandBranch("greeting")]
internal sealed class GreetingCommandAttribute : CommandAttribute
{
    public GreetingCommandAttribute(string verb)
        : base(verb) { }
}
