using System.Runtime.CompilerServices;

namespace XO.Console.Cli;

internal static class Initializer
{
    [ModuleInitializer]
    public static void Initialize()
        => VerifySourceGenerators.Initialize();
}
