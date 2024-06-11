using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XO.Console.Cli.Model;

internal readonly record struct LocationInfo(
    string FilePath,
    TextSpan TextSpan,
    LinePositionSpan LineSpan)
{
    public static implicit operator LocationInfo?(Location location)
    {
        if (location.SourceTree is null)
            return null;

        return new LocationInfo(
            location.SourceTree.FilePath,
            location.SourceSpan,
            location.GetLineSpan().Span);
    }

    public Location ToLocation()
        => Location.Create(FilePath, TextSpan, LineSpan);
}
