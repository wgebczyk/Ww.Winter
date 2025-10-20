using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ww.Winter.Generator.Model;

public record TypeModel(
    string Namespace,
    string Name,
    string FullyQualifiedName,
    ImmutableArray<string> ParentTypes
)
{
    public static TypeModel? FromSyntax(BaseTypeDeclarationSyntax syntax)
    {
        var parentTypes = new List<string>(1);
        var parentNamespaces = new List<string>(2);

        SyntaxNode? node = syntax.Parent;
        while (node is not null)
        {
            if (node is BaseTypeDeclarationSyntax cSyntax)
            {
                var identifier = cSyntax.Identifier.ToString();
                parentTypes.Add(identifier);
            }
            else if (node is BaseNamespaceDeclarationSyntax nsSyntax)
            {
                var identifier = nsSyntax.Name.ToString();
                parentNamespaces.Add(identifier);
            }
            node = node.Parent;
        }

        parentTypes.Reverse();
        parentNamespaces.Reverse();

        var name = syntax.Identifier.ToString();
        var ns = string.Join(".", parentNamespaces);
        string[] allParts = [.. parentNamespaces, .. parentTypes, name];
        var fullyQualifiedName = string.Join(".", allParts);
        return new TypeModel(ns, name, fullyQualifiedName, [.. parentTypes]);
    }
}
