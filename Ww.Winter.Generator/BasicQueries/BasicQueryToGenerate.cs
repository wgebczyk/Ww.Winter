using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public record BasicQueryToGenerate(
    TypeModel OwnedBy,
    ImmutableArray<BasicQuery> Queries
)
{
    public static BasicQueryToGenerate Create(SemanticModel semanticModel, TypeDeclarationSyntax ownedBySyntax)
    {
        var queries = new List<BasicQuery>();
        foreach (var attribute in ownedBySyntax.AttributeLists.SelectMany(x => x.Attributes))
        {
            // do as much as possible syntax checks first for performance
            string name;
            var nameSyntax = attribute.Name;
            if (nameSyntax is QualifiedNameSyntax qualifiedName)
            {
                name = qualifiedName.Right.Identifier.ValueText;
            }
            else if (nameSyntax is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }
            else
            {
                throw new InvalidOperationException($"INTERNAL ERROR: Unknown attribute name syntax: '{nameSyntax.GetType().Name}'.");
            }
            if (name != "BasicQuery" && name != "BasicQueryAttribute")
            {
                continue;
            }

            queries.Add(BasicQuery.Create(semanticModel, attribute));
        }

        return new BasicQueryToGenerate(
            OwnedBy: TypeModel.FromSyntax(ownedBySyntax),
            Queries: [..queries]
        );
    }
}
