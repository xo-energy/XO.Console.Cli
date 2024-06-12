using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace XO.Console.Cli.Generators;

public sealed class AssemblyCommandAppDefaultsGeneratorTest
{
    [Fact]
    public Task Generates()
    {
        return VerifySource(
            """
            namespace Test;
            """);
    }

    [Fact]
    public Task GeneratesDescriptionFromDescriptionAttribute()
    {
        return VerifySource(
            """
            using System.Reflection;

            [assembly: AssemblyDescription("A test assembly")]

            namespace Test;
            """);
    }

    [Fact]
    public Task GeneratesVersionFromInformationalVersionAttribute()
    {
        return VerifySource(
            """
            using System.Reflection;

            [assembly: AssemblyInformationalVersion("1.2.3-beta.3")]

            namespace Test;
            """);
    }

    [Fact]
    public Task GeneratesVersionFromInformationalVersionAttribute_WhenVersionAttributeExists()
    {
        return VerifySource(
            """
            using System.Reflection;

            [assembly: AssemblyInformationalVersion("1.2.3-beta.3")]
            [assembly: AssemblyVersion("1.2.3")]

            namespace Test;
            """);
    }

    [Fact]
    public Task GeneratesVersionFromVersionAttribute()
    {
        return VerifySource(
            """
            using System.Reflection;

            [assembly: AssemblyVersion("1.2.3")]

            namespace Test;
            """);
    }

    [Fact]
    public async Task SetsEntryAssemblyAttributes()
    {
        var source =
            """
            using System.Reflection;
            using System.Threading.Tasks;
            using XO.Console.Cli;

            [assembly: AssemblyDescription("A test assembly")]
            [assembly: AssemblyInformationalVersion("1.2.3-beta.3")]
            [assembly: AssemblyVersion("1.2.3")]

            namespace Test;

            public static class Program
            {
                public static async Task<int> Main(string[] args)
                {
                    return await new CommandAppBuilder()
                        .ExecuteAsync(args);
                }
            }
            """;

        var generator = new AssemblyCommandAppDefaultsGenerator();
        var compilation = CompilationHelper.CreateCompilation(source);

        // make sure the input source compiles without warnings before involving the generator
        Assert.Empty(compilation.GetDiagnostics());

        _ = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var outputDirectoryName = Path.GetRandomFileName();
        var outputDirectoryPath = Path.Join(Path.GetTempPath(), outputDirectoryName);
        var assemblyPath = Path.Join(outputDirectoryPath, "Test.dll");

        Directory.CreateDirectory(outputDirectoryPath);
        try
        {
            EmitResult emitResult;
            string stdout;
            string stderr;

            // compile the source to an assembly on disk
            using (var stream = File.OpenWrite(assemblyPath))
                emitResult = outputCompilation.Emit(stream);

            Assert.True(emitResult.Success);
            Assert.Empty(emitResult.Diagnostics);

            // copy referenced assemblies to the output folder
            foreach (var reference in outputCompilation.ExternalReferences.OfType<PortableExecutableReference>())
            {
                if (reference.FilePath != null)
                    File.Copy(reference.FilePath, Path.Combine(outputDirectoryPath, Path.GetFileName(reference.FilePath)), true);
            }

            // write a runtimeconfig.json that tells dotnet the assembly is framework-dependent
            File.WriteAllText(
                Path.Join(outputDirectoryPath, "Test.runtimeconfig.json"),
                """
                {
                    "runtimeOptions": {
                        "tfm": "net8.0",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "8.0.0"
                        }
                    }
                }
                """);

            // run the assembly with --help and capture stdout and stderr
            var procstart = new ProcessStartInfo("dotnet", ["exec", assemblyPath, "--help"])
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var proc = Process.Start(procstart))
            {
                Assert.NotNull(proc);
                stdout = await proc.StandardOutput.ReadToEndAsync();
                stderr = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();
                Assert.Equal(0, proc.ExitCode);
            }

            Assert.Empty(stderr);

            await Verify(stdout);
        }
        finally
        {
            Directory.Delete(outputDirectoryPath, recursive: true);
        }
    }

    private static async Task VerifySource(string source)
    {
        var driver = CompilationHelper.RunGeneratorAndAssertEmptyDiagnostics<AssemblyCommandAppDefaultsGenerator>(source);

        await Verify(driver)
            .ScrubGeneratedCodeAttribute()
            ;
    }
}
