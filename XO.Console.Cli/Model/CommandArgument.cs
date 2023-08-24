namespace XO.Console.Cli.Model;

public sealed class CommandArgument : CommandParameter
{
    public CommandArgument(
        CommandArgumentAttribute attribute,
        Type declaringType,
        Type valueType,
        Action<CommandContext, object?> setter,
        string? description = null)
        : base(declaringType, valueType, setter, description)
    {
        this.Attribute = attribute;
    }

    public override string Name
        => Attribute.Name;

    public CommandArgumentAttribute Attribute { get; }
}
