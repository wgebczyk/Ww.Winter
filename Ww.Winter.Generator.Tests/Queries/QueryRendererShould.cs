using FluentAssertions;
using Ww.Winter.Generator.Primitives;
using Ww.Winter.Generator.Queries;

namespace Ww.Winter.Generator.Tests.Queries;

public sealed class QueryRendererShould
{
    [Fact]
    public void RenderQueryInBaseSetup()
    {
        var (Content, HintName) = QueryRenderer.Render(new QueryToGenerate(
            CreateOwnedByType(),
            [], [new Query(
                CreateEntity(),
                CreateFilterWithAllComparisons(),
                "FindIt",
                "Task<IList<MyEntity>>",
                "filter",
                "sort",
                "pagination",
                UseBaseQuery: null
            )]
        ));

        HintName.Should().Be("MyNs.MyQueryClass_Queries.g.cs");

        Content.Should().Contain("if (filter.ThridPropIs is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp == filter.ThridPropIs)");
        Content.Should().Contain("if (filter.ThridPropIsNot is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp != filter.ThridPropIsNot)");
        Content.Should().Contain("if (filter.ThridPropFragment is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp.Contains(filter.ThridPropFragment))");
        Content.Should().Contain("if (filter.ThridPropPrefix is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp.StartsWith(filter.ThridPropPrefix))");
        Content.Should().Contain("if (filter.ThridPropSuffix is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp.EndsWith(filter.ThridPropSuffix))");
        Content.Should().Contain("if (filter.FirstPropFrom is not null)");
        Content.Should().Contain("query = query.Where(e => e.FirstProp >= filter.FirstPropFrom)");
        Content.Should().Contain("if (filter.FirstPropGreaterThan is not null)");
        Content.Should().Contain("query = query.Where(e => e.FirstProp > filter.FirstPropGreaterThan)");
        Content.Should().Contain("if (filter.FirstPropLessThan is not null)");
        Content.Should().Contain("query = query.Where(e => e.FirstProp < filter.FirstPropLessThan)");
        Content.Should().Contain("if (filter.FirstPropTo is not null)");
        Content.Should().Contain("query = query.Where(e => e.FirstProp <= filter.FirstPropTo)");

        Content.Should().Contain("query = ApplySort(query, sort)");
        Content.Should().Contain("query = ApplyPagination(query, pagination)");
        Content.Should().Contain("IQueryable<MyEntity> ApplySort(IQueryable<MyEntity> query, SortParams sort)");
        Content.Should().Contain("IQueryable<MyEntity> ApplyPagination(IQueryable<MyEntity> query, PaginationParams pagination)");
    }

    [Fact]
    public void RenderQueryClassForNestedClasssesCase()
    {
        var (Content, HintName) = QueryRenderer.Render(new QueryToGenerate(
            new TypeModel("MyNs", "MyQueryClass", "MyNs.MyQueryClass", true, [new TypeNameModel("Outer", false), new TypeNameModel("Root", true)]),
            [], [new Query(
                CreateEntity(),
                CreateFilter(),
                "FindIt",
                "Task<IList<MyEntity>>",
                "filter",
                "sort",
                "pagination",
                UseBaseQuery: null
            )]
        ));

        HintName.Should().Be("MyNs.MyQueryClass_Queries.g.cs");
        Content.Should().Contain("partial class Outer");
        Content.Should().Contain("partial record Root");
        Content.Should().Contain("partial record MyQueryClass");
        Content.Should().Contain("public partial async Task<IList<MyEntity>> FindIt");
    }

    [Fact]
    public void RenderQueryClassWhenCustomMethodsAreDefined()
    {
        var query = CreateQuery();
        var (Content, HintName) = QueryRenderer.Render(new QueryToGenerate(
            CreateOwnedByType(),
            [
                new MethodModel("ApplyFirstPropLessThan", CreateQueryableType("MyEntity")),
                new MethodModel("ApplySecondPropIs", CreateQueryableType("MyEntity")),
            ],
            [query]
        ));

        HintName.Should().Be("MyNs.MyQueryClass_Queries.g.cs");
        Content.Should().Contain("query = ApplyFirstPropLessThan(query, filter.FirstPropLessThan)");
        Content.Should().Contain("query = ApplySecondPropIs(query, filter.SecondPropIs)");

        Content.Should().Contain("if (filter.ThridPropPrefix is not null)");
        Content.Should().Contain("query = query.Where(e => e.ThridProp.StartsWith(filter.ThridPropPrefix))");
    }

    [Fact]
    public void RenderQueryClassWithCustomProjection()
    {
        var query = CreateQuery();
        var (Content, HintName) = QueryRenderer.Render(new QueryToGenerate(CreateOwnedByType(), [new MethodModel("FindItProjectTo", query.Entity.Type)], [query]));

        HintName.Should().Be("MyNs.MyQueryClass_Queries.g.cs");
        Content.Should().Contain("return await query");
        Content.Should().Contain(".Select(FindItProjectTo)");
        Content.Should().Contain(".ToListAsync(cancellationToken)");
    }

    [Fact]
    public void RenderQueryClassWithCustomBaseQuery()
    {
        var query = CreateQuery();
        var (Content, HintName) = QueryRenderer.Render(new QueryToGenerate(CreateOwnedByType(), [], [query with { UseBaseQuery = "GetMyBaseQuery" }]));

        HintName.Should().Be("MyNs.MyQueryClass_Queries.g.cs");
        Content.Should().Contain("var query = GetMyBaseQuery()");
    }

    private static TypeModel CreateOwnedByType()
    {
        return new TypeModel("MyNs", "MyQueryClass", "MyNs.MyQueryClass", true, []);
    }
    private static Query CreateQuery()
    {
        return new Query(
            CreateEntity(),
            CreateFilter(),
            "FindIt",
            "Task<IList<MyEntity>>",
            "filter",
            "sort",
            "pagination",
            UseBaseQuery: null
        );
    }
    private static EntityModel CreateEntity()
    {
        return new EntityModel(
            new TypeModel("EntityNs", "MyEntity", "EntityNs.MyEntity", true, []),
            [
                new PropertyModel("FirstProp", new PropertyTypeModel("int", false, null)),
                new PropertyModel("SecondProp", new PropertyTypeModel("bool", false, null)),
                new PropertyModel("ThridProp", new PropertyTypeModel("string", false, null)),
            ],
            []
        );
    }
    private static EntityModel CreateFilter()
    {
        return new EntityModel(
            new TypeModel("FilterNs", "MyFilter", "FilterNs.MyFilter", true, []),
            [
                new PropertyModel("FirstPropLessThan", new PropertyTypeModel("int", true, null)),
                new PropertyModel("SecondPropIs", new PropertyTypeModel("bool", true, null)),
                new PropertyModel("ThridPropPrefix", new PropertyTypeModel("string", true, null)),
            ],
            []
        );
    }
    private static EntityModel CreateFilterWithAllComparisons()
    {
        return new EntityModel(
            new TypeModel("FilterNs", "MyFilterFull", "FilterNs.MyFilterFull", true, []),
            [
                new PropertyModel("ThridPropIs", new PropertyTypeModel("string", true, null)),
                new PropertyModel("ThridPropIsNot", new PropertyTypeModel("string", true, null)),
                new PropertyModel("ThridPropFragment", new PropertyTypeModel("string", true, null)),
                new PropertyModel("ThridPropPrefix", new PropertyTypeModel("string", true, null)),
                new PropertyModel("ThridPropSuffix", new PropertyTypeModel("string", true, null)),

                new PropertyModel("FirstPropFrom", new PropertyTypeModel("int", true, null)),
                new PropertyModel("FirstPropGreaterThan", new PropertyTypeModel("int", true, null)),
                new PropertyModel("FirstPropLessThan", new PropertyTypeModel("int", true, null)),
                new PropertyModel("FirstPropTo", new PropertyTypeModel("int", true, null)),
            ],
            []
        );
    }
    private static TypeModel CreateQueryableType(string entityName)
    {
        return new TypeModel("", $"IQueryable<{entityName}>", "IQueryable<{entityName}>", false, []);
    }
}
