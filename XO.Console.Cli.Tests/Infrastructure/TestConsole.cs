using System;
using System.IO;
using System.Text;

namespace XO.Console.Cli;

public sealed class TestConsole : IConsole, IDisposable
{
    public TestConsole()
    {
        this.OutputBuffer = new StringBuilder();
        this.Output = new StringWriter(this.OutputBuffer);
        this.ErrorBuffer = new StringBuilder();
        this.Error = new StringWriter(this.ErrorBuffer);
    }

    public void Dispose()
    {
        this.Output.Dispose();
        this.Error.Dispose();
    }

    public StringBuilder OutputBuffer { get; }
    public StringBuilder ErrorBuffer { get; }
    public TextReader Input => throw new NotSupportedException();
    public TextWriter Output { get; }
    public TextWriter Error { get; }
    public bool IsInputRedirected => false;
    public bool IsOutputRedirected => true;
    public bool IsErrorRedirected => true;
}
