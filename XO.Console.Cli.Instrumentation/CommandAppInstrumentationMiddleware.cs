using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace XO.Console.Cli.Instrumentation;

/// <summary>
/// An <see cref="ICommandApp"/> middleware that wraps command execution in an <see cref="Activity"/>.
/// </summary>
public sealed class CommandAppInstrumentationMiddleware : ICommandAppMiddleware
{
    /// <summary>
    /// The name of the <see cref="ActivitySource"/> for activities started by this middleware.
    /// </summary>
    public const string ActivitySourceName = ThisAssembly.AssemblyName;

    private static readonly ActivitySource Source
        = new(ActivitySourceName, version: ThisAssembly.AssemblyInformationalVersion);

    private readonly string _defaultName;
    private readonly CommandAppInstrumentationOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandAppInstrumentationMiddleware"/>.
    /// </summary>
    /// <param name="env">The hosting environment.</param>
    /// <param name="optionsAccessor">The configuration options.</param>
    public CommandAppInstrumentationMiddleware(IHostEnvironment env, IOptions<CommandAppInstrumentationOptions> optionsAccessor)
    {
        _defaultName = env.ApplicationName;
        _options = optionsAccessor.Value;
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(ExecutorDelegate next, CommandContext context, CancellationToken cancellationToken)
    {
        int? result = null;
        var name = String.Join(' ', context.ParseResult.GetVerbs());
        if (String.IsNullOrEmpty(name))
            name = _defaultName;

        var activity = Source.StartActivity(name, _options.ActivityKind);
        try
        {
            if (activity?.IsAllDataRequested == true)
            {
                activity.AddTag(TraceSemanticConventions.AttributeCodeFunction, nameof(AsyncCommand.ExecuteAsync));
                activity.AddTag(TraceSemanticConventions.AttributeCodeNamespace, context.Command.GetType().FullName);
                _options.EnrichWithICommandContext?.Invoke(activity, context);
            }

            result = await next(context, cancellationToken)
                .ConfigureAwait(false);

            activity?.AddTag("command.exit_code", result);
        }
        catch (Exception ex)
        {
            if (activity?.IsAllDataRequested == true)
            {
                activity.RecordException(ex);
                activity.SetStatus(Status.Error.WithDescription(ex.Message));
            }
            throw;
        }
        finally
        {
            activity?.Dispose();
        }

        return result.Value;
    }
}
