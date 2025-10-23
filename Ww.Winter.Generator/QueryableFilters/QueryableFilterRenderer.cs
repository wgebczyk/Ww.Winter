using System.Linq;
using Ww.Winter.Generator.Model;
using Ww.Winter.Generator.Parsing;
using Ww.Winter.Generator.Rendering;

namespace Ww.Winter.Generator.QueryableFilters;

public sealed class QueryableFilterRenderer: SourceRenderer
{
    private QueryableFilterRenderer()
    {
    }

    private void RenderCore(QueryableFilterToGenerate toGenerate)
    {
        var filter = toGenerate.Filter;
        var entity = toGenerate.Entity;
        var propertyParser = new FilterPropertyIdentifierParser();

        WriteLine($"using System.Linq;");
        WriteLine($"using {entity.Type.Namespace};");
        WriteLine();
        WriteLine($"#nullable enable");
        WriteLine();
        WriteLine($"namespace {filter.Type.Namespace};");
        WriteLine();
        foreach (var parentType in filter.Type.ParentTypes)
        {
            WriteLine($"partial class {parentType}");
            WriteOpenBracket();
        }
        WriteLine($"public partial class {filter.Type.Name}");
        WriteOpenBracket();

        WriteLine($"public IQueryable<{entity.Type.Name}> ApplyFilter(IQueryable<{entity.Type.Name}> query)");
        WriteOpenBracket();
        foreach (var property in filter.Properties)
        {
            if (!propertyParser.TryParse(entity, property.Name, out var filterProperty))
            {
                WriteLine($"// WARN: Unable to process filter property '{property.Name}' for entity '{entity.Type.Name}'");
                continue;
            }
            var entityProperty = filterProperty.Properties.Single().Name;

            WriteLine($"if (this.{property.Name} is not null)");
            WriteOpenBracket();
            switch (filterProperty!.Comparison)
            {
                case FilterComparison.Equals:
                    WriteLine($"query = query.Where(e => e.{entityProperty} == this.{property.Name});");
                    break;
                case FilterComparison.NotEquals:
                    WriteLine($"query = query.Where(e => e.{entityProperty} != this.{property.Name});");
                    break;
                case FilterComparison.GreaterThan:
                    WriteLine($"query = query.Where(e => e.{entityProperty} > this.{property.Name});");
                    break;
                case FilterComparison.GreaterThanOrEqual:
                    WriteLine($"query = query.Where(e => e.{entityProperty} >= this.{property.Name});");
                    break;
                case FilterComparison.LessThan:
                    WriteLine($"query = query.Where(e => e.{entityProperty} < this.{property.Name});");
                    break;
                case FilterComparison.LessThanOrEqual:
                    WriteLine($"query = query.Where(e => e.{entityProperty} <= this.{property.Name});");
                    break;
                case FilterComparison.Contains:
                    WriteLine($"query = query.Where(e => e.{entityProperty}.Contains(this.{property.Name}));");
                    break;
                case FilterComparison.StartsWith:
                    WriteLine($"query = query.Where(e => e.{entityProperty}.StartsWith(this.{property.Name}));");
                    break;
                case FilterComparison.EndsWith:
                    WriteLine($"query = query.Where(e => e.{entityProperty}.EndsWith(this.{property.Name}));");
                    break;
            }
            WriteCloseBracket();
        }
        WriteLine();
        WriteLine($"return query;");
        WriteCloseBracket();
        WriteCloseBracket();
        foreach (var parentType in filter.Type.ParentTypes)
        {
            WriteCloseBracket();
        }
    }

    public static (string Content, string HintName) Render(QueryableFilterToGenerate toGenerate)
    {
        var renderer = new QueryableFilterRenderer();
        renderer.RenderCore(toGenerate);
        var content = renderer.GetSource();

        var filename = Helpers.ToSafeFileName(toGenerate.Entity.Type.FullyQualifiedName, "QueryableFilters");
        return (content, filename);
    }
}