﻿//HintName: AssemblyCommandAppDefaults.g.cs
// <auto-generated/>
using XO.Console.Cli.Infrastructure;

#nullable enable

namespace XO.Console.Cli.Generated;

/// <summary>
/// This class was auto-generated to support the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("XO.Console.Cli.Analyzers", "{ThisAssembly.AssemblyVersion}")]
[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
internal static class AssemblyCommandAppDefaults
{
    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    public static void Initialize()
    {
        var entryAssembly = global::System.Reflection.Assembly.GetEntryAssembly();
        var executingAssembly = global::System.Reflection.Assembly.GetExecutingAssembly();
        if (executingAssembly == entryAssembly)
        {
            TypeRegistry.SetEntryAssemblyProperties(
                assemblyName: "Test",
                assemblyDescription: null,
                assemblyVersion: "1.2.3-beta.3");
        }
    }
}