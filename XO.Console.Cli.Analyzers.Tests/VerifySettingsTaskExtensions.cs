namespace XO.Console.Cli;

internal static partial class VerifySettingsTaskExtensions
{
    private static readonly string AnalyzerAssemblyVersion
        = typeof(DiagnosticDescriptors).Assembly.GetName().Version!.ToString();

    public static SettingsTask ScrubGeneratedCodeAttribute(this SettingsTask task)
    {
        return task
            .AddScrubber("cs", static (buffer) => buffer.Replace($"\"{AnalyzerAssemblyVersion}\"", "\"{ThisAssembly.AssemblyVersion}\""))
            ;
    }
}
