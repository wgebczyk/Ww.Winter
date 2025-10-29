using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Queries;

public sealed record Query(
    EntityModel Entity,
    EntityModel Filter,
    string MethodName,
    string MethodReturnTypeExpression,
    string FilterParamName,
    string? SortParamName,
    string? PaginationParamName,

    string? UseBaseQueryExpression
)
{
    public static (TypeModel, ImmutableHashSet<MethodModel>, Query) CreateQueryElements(SemanticModel semanticModel, MethodDeclarationSyntax syntax)
    {
        var parentSyntax = syntax.Parent;
        if (parentSyntax is not TypeDeclarationSyntax typeDeclarationSyntax)
        {
            throw new InvalidOperationException($"[Query(...)] attributed method does not belong to type.");
        }

        var query = Create(semanticModel, syntax, typeDeclarationSyntax);
        var ownedByType = TypeModel.FromSyntax(typeDeclarationSyntax);
        var ownedByMethods = parentSyntax.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(x => !x.Modifiers.Any(y => y.IsKind(SyntaxKind.PartialKeyword)))
            .Select(x => MethodModel.FromSyntax(semanticModel, x))
            .ToImmutableHashSet();
        return (ownedByType, ownedByMethods, query);
    }

    public static Query Create(SemanticModel semanticModel, MethodDeclarationSyntax syntax, TypeDeclarationSyntax ownedBySyntax)
    {
        var attribute = syntax.AttributeLists.SelectMany(x => x.Attributes).Single();
        var entity = FromAttributeArgument(semanticModel, attribute, 0);

        var parameters = syntax.ParameterList.Parameters;

        var filterParameter = parameters.Single(x => x.Identifier.ValueText.Contains("filter", StringComparison.OrdinalIgnoreCase));
        var sortParameter = parameters.SingleOrDefault(x => x.Identifier.ValueText.Contains("sort", StringComparison.OrdinalIgnoreCase));
        var paginationParameter = parameters.SingleOrDefault(x => x.Identifier.ValueText.Contains("pagination", StringComparison.OrdinalIgnoreCase));

        if (filterParameter.Type is null)
        {
            throw new InvalidOperationException($"Missing named symbol from filter parameter. {filterParameter}.");
        }

        var filterSymbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, filterParameter.Type)
            ?? throw new InvalidOperationException("Missing named symbol for parameter type. {filterParameter.Type}");
        var filter = EntityModel.FromSymbol(filterSymbol, 1);

        var useBaseQueryExpression =
            GetUseBaseQueryFromAttribute(attribute) ??
            GetUseBaseQueryFromOwnedBy(semanticModel, ownedBySyntax, entity);

        return new Query(
            entity,
            filter,
            syntax.Identifier.ValueText,
            syntax.ReturnType.ToString(),
            filterParameter.Identifier.ValueText,
            sortParameter.Identifier.ValueText,
            paginationParameter.Identifier.ValueText,
            useBaseQueryExpression
        );
    }

    private static EntityModel FromAttributeArgument(
        SemanticModel semanticModel,
        AttributeSyntax attribute,
        int argumentIndex
    )
    {
        var expression = TypeModel.TryGetTypeOfExpression(attribute, argumentIndex)
            ?? throw new InvalidOperationException($"Cannot find n-th ({argumentIndex}) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax)
        {
            throw new InvalidOperationException("Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("Missing named symbol.");
        return EntityModel.FromSymbol(symbol, 0);
    }

    private static string? GetUseBaseQueryFromAttribute(AttributeSyntax attribute)
    {
        var argumentList = attribute.ArgumentList
            ?? throw new InvalidOperationException("Cannot find attribute's argument list.");

        var useBaseQueryArgument = argumentList.Arguments.SingleOrDefault(x => x.NameEquals?.Name.Identifier.ValueText == "UseBaseQueryExpression");
        if (useBaseQueryArgument is null)
        {
            return null;
        }
        if (useBaseQueryArgument.Expression is LiteralExpressionSyntax literalExpression)
        {
            return literalExpression.Token.ValueText;
        }
        if (useBaseQueryArgument.Expression is InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression is IdentifierNameSyntax nameSyntax && nameSyntax.Identifier.ValueText == "nameof")
            {
                var firstArgument = invocationExpression.ArgumentList.Arguments.Single();
                if (firstArgument.Expression is IdentifierNameSyntax argumentIdentifierExpression)
                {
                    return argumentIdentifierExpression.Identifier.ValueText;
                }
            }
        }

        throw new InvalidOperationException("Unsupported attribute value expression.");
    }

    private static string? GetUseBaseQueryFromOwnedBy(SemanticModel semanticModel, TypeDeclarationSyntax syntax, EntityModel entity)
    {
        foreach (var property in syntax.Members.OfType<PropertyDeclarationSyntax>())
        {
            var symbolInfo = semanticModel.GetSymbolInfo(property.Type);
            if (symbolInfo.Symbol is ITypeSymbol typeSymbol)
            {
                var candidateProperty = GetUseBaseQueryFromDbContext(semanticModel, typeSymbol, entity);
                if (candidateProperty is not null)
                {
                    return $"{property.Identifier.ValueText}.{candidateProperty}.AsNoTracking()";
                }
            }
        }
        foreach (var field in syntax.Members.OfType<FieldDeclarationSyntax>().SelectMany(x => x.Declaration.Variables))
        {
            var symbolInfo = semanticModel.GetDeclaredSymbol(field);
            if (symbolInfo is IFieldSymbol fieldSymbol)
            {
                var candidateField = GetUseBaseQueryFromDbContext(semanticModel, fieldSymbol.Type, entity);
                if (candidateField is not null)
                {
                    return $"{field.Identifier.ValueText}.{candidateField}.AsNoTracking()";
                }
            }
        }

        return null;
    }

    private static string? GetUseBaseQueryFromDbContext(SemanticModel semanticModel, ITypeSymbol typeSymbol, EntityModel entity)
    {
        var expectedDbSetType = $"Microsoft.EntityFrameworkCore.DbSet<{entity.Type.FullyQualifiedName}>";

        var candidateProperty = typeSymbol.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(x => x.Type.ToString() == expectedDbSetType);
        if (candidateProperty is not null)
        {
            return candidateProperty.Name;
        }
        return null;
    }
}
