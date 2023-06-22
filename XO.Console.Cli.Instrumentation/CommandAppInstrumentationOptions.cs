using System.Diagnostics;

namespace XO.Console.Cli.Instrumentation;

/// <summary>
/// Configures OpenTelemetry instrumentation for <see cref="ICommandApp"/>.
/// </summary>
public sealed class CommandAppInstrumentationOptions
{
    /// <summary>
    /// The <see cref="System.Diagnostics.ActivityKind"/> to assign to command execution activities.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="ActivityKind.Producer"/>. Before changing the activity kind, check whether your
    /// environment assigns special meaning to certain kinds.
    /// </remarks>
    public ActivityKind ActivityKind { get; set; } = ActivityKind.Producer;

    /// <summary>
    /// A delegate that enriches the <see cref="Activity"/> with information from the execution context.
    /// </summary>
    public Action<Activity, CommandContext>? EnrichWithCommandContext { get; set; }
}
