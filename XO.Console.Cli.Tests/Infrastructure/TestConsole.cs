namespace XO.Console.Cli.Infrastructure;

public sealed class TestConsole : IConsole, IDisposable
{
    private readonly MemoryStream _output = new();
    private readonly MemoryStream _error = new();

    public TestConsole()
    {
        this.Output = new StreamWriter(_output);
        this.Error = new StreamWriter(_error);
    }

    public void Dispose()
    {
        this.Output.Dispose();
        this.Error.Dispose();
    }

    public TextReader Input => throw new NotSupportedException();
    public TextWriter Output { get; }
    public TextWriter Error { get; }
    public bool IsInputRedirected => false;
    public bool IsOutputRedirected => true;
    public bool IsErrorRedirected => true;
    public Stream OpenStandardError() => new WrapperStream(_error);
    public Stream OpenStandardInput() => throw new NotSupportedException();
    public Stream OpenStandardOutput() => new WrapperStream(_output);

    public string ReadErrorAsString()
        => ReadAsString(this.Error, _error);

    public string ReadOutputAsString()
        => ReadAsString(this.Output, _output);

    private static string ReadAsString(TextWriter writer, MemoryStream stream)
    {
        writer.Flush();
        stream.Position = 0;
        using (var reader = new StreamReader(stream, leaveOpen: true))
            return reader.ReadToEnd();
    }

    private sealed class WrapperStream(Stream stream) : Stream
    {
        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set => stream.Position = value; }
        public override void Flush() => stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void SetLength(long value) => stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
        protected override void Dispose(bool disposing)
        {
            // Do not dispose the underlying stream.
        }
    }
}
