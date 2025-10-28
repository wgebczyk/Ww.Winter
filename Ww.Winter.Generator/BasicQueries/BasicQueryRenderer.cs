using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public sealed class BasicQueryRenderer : SourceRenderer
{
    private BasicQueryRenderer()
    {
    }

    private void RenderCore(BasicQueryToGenerate toGenerate)
    {
        var queryParser = new BasicEntityQueryIdentifierParser();

        WriteLine($"using Microsoft.EntityFrameworkCore;");
        WriteLine($"using Ww.Winter;");
        WriteLine();
        WriteLine($"#nullable enable");
        WriteLine();
        WriteStartClass(toGenerate.OwnedBy);
        foreach (var query in toGenerate.Queries)
        {
            if (!queryParser.TryParse(query.Entity, query.MethodName, out var entityQuery))
            {
                WriteLine($"// WARN: Unable to process basic query '{query.MethodName}' for entity '{query.Entity.Type.Name}'");
                continue;
            }
            WriteLine($"public async Task<{query.Entity.Type.Name}> {query.MethodName}(");
            foreach (var filterProperty in entityQuery.FilterProperties)
            {
                var property = filterProperty.Properties.Single();
                WriteLine($"    {property.Type.Name}? {ToCamelCase(property.Name)},");
            }
            WriteLine($"    CancellationToken cancellationToken)");
            WriteOpenBracket();
            if (query.UseBaseQuery is not null)
            {
                WriteLine($"var query = query.UseBaseQuery()");
                WriteLine($"    .TagWith(\"{query.MethodName}\");");
            }
            else
            {
                WriteLine($"var query = this.dbContext.{query.Entity.Type.Name}s");
                WriteLine($"    .AsNoTracking()");
                WriteLine($"    .TagWith(\"{query.MethodName}\");");
            }
            WriteLine();
            foreach (var filterProperty in entityQuery.FilterProperties)
            {
                var property = filterProperty.Properties.Single();
                WriteLine($"if ({ToCamelCase(property.Name)} is not null)");
                WriteOpenBracket();
                WriteLine($"query = query.Where(e => e.{property.Name} == {ToCamelCase(property.Name)});");
                WriteCloseBracket();
            }
            WriteLine();
            WriteLine($"var result = await query.SingleOrDefaultAsync(cancellationToken);");
            WriteLine($"if (result is null)");
            WriteOpenBracket();
            WriteLine($"throw new NotFoundEntityException(\"{query.Entity.Type.Name}\", \"Id lookup failure\");");
            WriteCloseBracket();
            WriteLine($"return result;");
            WriteCloseBracket();
        }
        WriteEndClass(toGenerate.OwnedBy);
    }
    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }
        if (value.Length == 1)
        {
            return value.ToLowerInvariant();
        }
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static (string Content, string HintName) Render(BasicQueryToGenerate toGenerate)
    {
        var renderer = new BasicQueryRenderer();
        renderer.RenderCore(toGenerate);
        var content = renderer.GetSource();

        var filename = ToSafeFileName(toGenerate.OwnedBy.FullyQualifiedName, "BasicQueries");
        return (content, filename);
    }
}