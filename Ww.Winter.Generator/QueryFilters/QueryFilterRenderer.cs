using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.QueryFilters;

public sealed class QueryFilterRenderer : SourceRenderer
{
    private QueryFilterRenderer()
    {
    }

    private void RenderCore(QueryFilterToGenerate toGenerate)
    {
        var propertyParser = new FilterPropertyIdentifierParser();
        var typeKeyword = toGenerate.OwnedBy.IsRecord ? "record" : "class";

        WriteLine($"using System.Linq;");
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
        WriteLine($"partial {(toGenerate.OwnedBy.IsRecord ? "record" : "class")} {toGenerate.OwnedBy.Name}");
        WriteOpenBracket();
        foreach (var queryFilter in toGenerate.QueryFilters)
        {
            var entityName = queryFilter.Entity.Type.Name;
            var filterName = queryFilter.Filter.Type.Name;
            WriteLine($"public static IQueryable<{entityName}> ApplyFilter(");
            WriteLine($"    this IQueryable<{entityName}> query,");
            WriteLine($"    {filterName} filter)");
            WriteOpenBracket();
            foreach (var property in queryFilter.Filter.Properties)
            {
                if (!propertyParser.TryParse(queryFilter.Entity, property.Name, out var filterProperty))
                {
                    WriteLine($"// WARN: Unable to process filter property '{property.Name}' for entity '{queryFilter.Entity.Type.Name}'");
                    continue;
                }
                var entityProperty = filterProperty.Properties.Single().Name;

                WriteLine($"if (filter.{property.Name} is not null)");
                WriteOpenBracket();
                switch (filterProperty!.Comparison)
                {
                    case FilterComparison.Equals:
                        WriteLine($"query = query.Where(e => e.{entityProperty} == filter.{property.Name});");
                        break;
                    case FilterComparison.NotEquals:
                        WriteLine($"query = query.Where(e => e.{entityProperty} != filter.{property.Name});");
                        break;
                    case FilterComparison.GreaterThan:
                        WriteLine($"query = query.Where(e => e.{entityProperty} > filter.{property.Name});");
                        break;
                    case FilterComparison.GreaterThanOrEqual:
                        WriteLine($"query = query.Where(e => e.{entityProperty} >= filter.{property.Name});");
                        break;
                    case FilterComparison.LessThan:
                        WriteLine($"query = query.Where(e => e.{entityProperty} < filter.{property.Name});");
                        break;
                    case FilterComparison.LessThanOrEqual:
                        WriteLine($"query = query.Where(e => e.{entityProperty} <= filter.{property.Name});");
                        break;
                    case FilterComparison.Contains:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.Contains(filter.{property.Name}));");
                        break;
                    case FilterComparison.StartsWith:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.StartsWith(filter.{property.Name}));");
                        break;
                    case FilterComparison.EndsWith:
                        WriteLine($"query = query.Where(e => e.{entityProperty}.EndsWith(filter.{property.Name}));");
                        break;
                }
                WriteCloseBracket();
            }
            WriteLine();
            WriteLine($"return query;");
            WriteCloseBracket();
        }
        WriteCloseBracket();
        foreach (var _ in toGenerate.OwnedBy.ParentTypes)
        {
            WriteCloseBracket();
        }
    }

    public static (string Content, string HintName) Render(QueryFilterToGenerate toGenerate)
    {
        var renderer = new QueryFilterRenderer();
        renderer.RenderCore(toGenerate);
        var content = renderer.GetSource();

        var filename = ToSafeFileName(toGenerate.OwnedBy.FullyQualifiedName, "QueryFilters");
        return (content, filename);
    }
}