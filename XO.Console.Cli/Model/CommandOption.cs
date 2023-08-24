namespace XO.Console.Cli.Model;

public sealed class CommandOption : CommandParameter
{
    public CommandOption(
        CommandOptionAttribute attribute,
        Type declaringType,
        Type valueType,
        Action<CommandContext, object?> setter,
        string? description = null,
        bool? isFlag = default)
        : base(declaringType, valueType, setter, description)
    {
        this.Attribute = attribute;
        this.IsFlag = isFlag ??
            this.ValueType == typeof(bool) ||
            this.ValueType == typeof(bool?) ||
            this.ValueType == typeof(bool[]) ||
            this.ValueType == typeof(bool?[]);
    }

    public CommandOptionAttribute Attribute { get; }
    public bool IsFlag { get; }

    public override string Name
        => this.Attribute.Name;

    public IEnumerable<string> GetNames()
    {
        yield return this.Attribute.Name;

        foreach (var alias in this.Attribute.Aliases)
            yield return alias;
    }
}
