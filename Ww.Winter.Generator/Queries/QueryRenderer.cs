using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Queries;

public sealed class QueryRenderer : SourceRenderer
{
    private QueryRenderer()
    {
    }

    private void RenderCore(QueryToGenerate toGenerate)
    {
        var propertyParser = new FilterPropertyIdentifierParser();

        WriteLine($"using System.Linq.Expressions;");
        WriteLine($"using Microsoft.EntityFrameworkCore;");
        WriteLine($"using Ww.Winter;");
        WriteLine();
        WriteLine($"#nullable enable");
        WriteLine();
        WriteLine($"namespace {toGenerate.OwnedBy.Namespace};");
        WriteLine();
        foreach (var parentType in toGenerate.OwnedBy.ParentTypes)
        {
            WriteLine($"partial {(parentType.IsRecord ? "record" : "class")} {parentType.Name}");
            WriteOpenBracket();
        }
        WriteLine($"partial class {toGenerate.OwnedBy.Name}");
        WriteOpenBracket();
        foreach (var query in toGenerate.Queries)
        {
            var filterParamName = query.FilterParamName;
            WriteLine($"public partial async Task<IList<{query.Entity.Type.Name}>> {query.MethodName}(");
            WriteLine($"    {query.Filter.Type.Name} {filterParamName},");
            WriteLine($"    SortParams {query.SortParamName},");
            WriteLine($"    PaginationParams {query.PaginationParamName},");
            WriteLine($"    CancellationToken cancellationToken)");
            WriteOpenBracket();
            WriteLine($"var query = this.dbContext.{query.Entity.Type.Name}s");
            WriteLine($"    .AsNoTracking()");
            WriteLine($"    .TagWith(\"{query.MethodName}\");");
            WriteLine();

            foreach (var property in query.Filter.Properties)
            {
                var customApplyMethodName = $"Apply{property.Name}";
                if (toGenerate.OwnedByMethods.Contains(customApplyMethodName))
                {
                    WriteLine($"query = {customApplyMethodName}(query, {filterParamName}.{property.Name});");
                    continue;
                }

                if (!propertyParser.TryParse(query.Entity, property.Name, out var filterProperty))
                {
                    WriteLine($"// WARN: Unable to process filter property '{property.Name}' for entity '{query.Entity.Type.Name}'");
                    continue;
                }
                var entityProperty = filterProperty.Properties.Single().Name;

                WriteLine($"if ({filterParamName}.{property.Name} is not null)");
                WriteOpenBracket();
                switch (filterProperty!.Comparison)
                {
                    case FilterComparison.Equals:
                        WriteLine($"query = query.Where(e => e.{entityProperty} == {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.NotEquals:
                        WriteLine($"query = query.Where(e => e.{entityProperty} != {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.GreaterThan:
                        WriteLine($"query = query.Where(e => e.{entityProperty} > {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.GreaterThanOrEqual:
                        WriteLine($"query = query.Where(e => e.{entityProperty} >= {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.LessThan:
                        WriteLine($"query = query.Where(e => e.{entityProperty} < {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.LessThanOrEqual:
                        WriteLine($"query = query.Where(e => e.{entityProperty} <= {filterParamName}.{property.Name});");
                        break;
                    case FilterComparison.Contains:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.Contains({filterParamName}.{property.Name}));");
                        break;
                    case FilterComparison.StartsWith:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.StartsWith({filterParamName}.{property.Name}));");
                        break;
                    case FilterComparison.EndsWith:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.EndsWith({filterParamName}.{property.Name}));");
                        break;
                }
                WriteCloseBracket();
            }
            WriteLine();
            WriteLine($"query = ApplySort(query, {query.SortParamName});");
            WriteLine($"query = ApplyPagination(query, {query.PaginationParamName});");
            WriteLine();
            WriteLine($"return await query.ToListAsync(cancellationToken);");
            WriteCloseBracket();
            WriteLine();
        }

        var generatedEntities = new HashSet<string>();
        foreach (var query in toGenerate.Queries)
        {
            var entity = query.Entity;
            if (!generatedEntities.Add(entity.Type.FullyQualifiedName))
            {
                continue;
            }

            WriteLine();
            WriteLine($"private IQueryable<{entity.Type.Name}> ApplySort(IQueryable<{entity.Type.Name}> query, SortParams sort)");
            WriteOpenBracket();
            WriteLine($"if (sort.Properties.Count == 0)");
            WriteOpenBracket();
            WriteLine($"return query;");
            WriteCloseBracket();
            WriteLine();
            WriteLine($"foreach (var property in sort.Properties)");
            WriteOpenBracket();
            WriteLine($"var propertyName = property.PropertyName.ToLowerInvariant();");
            WriteLine($"var direction = property.Direction;");
            WriteLine($"switch (propertyName)");
            WriteOpenBracket();
            foreach (var entityProperty in entity.Properties)
            {
                WriteLine($"case \"{entityProperty.Name.ToLowerInvariant()}\":");
                WriteLine($"    query = ApplySort(query, o => o.{entityProperty.Name}, direction == System.ComponentModel.ListSortDirection.Descending);");
                WriteLine($"    break;");
            }
            WriteCloseBracket();
            WriteCloseBracket();
            WriteLine($"return query;");
            WriteCloseBracket();
            WriteLine();
            WriteLine($"private static IOrderedQueryable<T> ApplySort<T, TKey>(");
            WriteLine($"    IQueryable<T> source,");
            WriteLine($"    Expression<Func<T, TKey>> keySelector,");
            WriteLine($"    bool descending)");
            WriteOpenBracket();
            WriteLine($"if (source is not IOrderedQueryable<T> ordered)");
            WriteOpenBracket();
            WriteLine($"return descending ? source.OrderByDescending(keySelector)");
            WriteLine($"                  : source.OrderBy(keySelector);");
            WriteCloseBracket();
            WriteLine($"return descending ? ordered.ThenByDescending(keySelector)");
            WriteLine($"                  : ordered.ThenBy(keySelector);");
            WriteCloseBracket();
            WriteLine();
            WriteLine($"private IQueryable<{entity.Type.Name}> ApplyPagination(IQueryable<{entity.Type.Name}> query, PaginationParams pagination)");
            WriteOpenBracket();
            WriteLine($"if (pagination.IsDefined)");
            WriteOpenBracket();
            WriteLine($"return query.Skip(pagination?.Skip ?? 0).Take(pagination?.Take ?? 10);");
            WriteCloseBracket();
            WriteLine($"return query;");
            WriteCloseBracket();
        }
        WriteCloseBracket();
        foreach (var _ in toGenerate.OwnedBy.ParentTypes)
        {
            WriteCloseBracket();
        }
    }

    public static (string Content, string HintName) Render(QueryToGenerate toGenerate)
    {
        var renderer = new QueryRenderer();
        renderer.RenderCore(toGenerate);
        var content = renderer.GetSource();

        var filename = ToSafeFileName(toGenerate.OwnedBy.FullyQualifiedName, "Queries");
        return (content, filename);
    }
}