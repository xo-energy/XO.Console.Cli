using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XO.Console.Cli.Model;

internal sealed record CommandDeclaration(
    ClassDeclarationSyntax SyntaxNode,
    string ParametersType);
