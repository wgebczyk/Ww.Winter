using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ww.Winter.Generator.Model;

namespace Ww.Winter.Generator.QueryFilters;

public sealed record QueryFilterToGenerate
{
    public TypeModel OwnedBy { get; }
    public ImmutableArray<QueryFilter> QueryFilters { get; }

    public QueryFilterToGenerate(
        TypeModel ownedBy,
        ImmutableArray<QueryFilter> queryFilters
    )
    {
        OwnedBy = ownedBy;
        QueryFilters = queryFilters;
    }

    public static QueryFilterToGenerate Create(SemanticModel semanticModel, ClassDeclarationSyntax ownedBy)
    {
        var queryFilters = new List<QueryFilter>();
        foreach (var attribute in ownedBy.AttributeLists.SelectMany(x => x.Attributes))
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
            if (name != "QueryFilter" && name != "QueryFilterAttribute")
            {
                continue;
            }

            queryFilters.Add(QueryFilter.Create(semanticModel, attribute));
        }
        return new QueryFilterToGenerate(
            ownedBy: TypeModel.FromSyntax(ownedBy),
            queryFilters: [.. queryFilters]
        );
    }
}
