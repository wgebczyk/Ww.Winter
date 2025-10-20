# ❄️ Ww.Winter - Generated Queries

While poking around Java Spring Boot, I stumbled upon... [JPA Repositories!](https://spring.io/guides/gs/accessing-data-jpa#_create_simple_queries)
They offer a super simple way to generate basic access to your storage entities in just a few steps:
- Create an interface like `CustomerRepository`
- Add as many methods as you need to access your storage
- Just... make sure to name them correctly!

Oh, and the best part?
You don't need to implement anything - proxy classes are generated for you automatically!

__Wow! Thank you, Spring Boot - this is awesome!__

## 🌍 `dotnet` World

If only we had a similar mechanism in .NET - imagine being able to generate 50-60% of your EF Core code automatically...

But nope! We've got code first, migrations, and all that jazz - yet nothing quite like this.
Hmm... wait a minute. We _do_ have source code generators. So why not build it ourselves?

```
Sure, it's basic and crude for now - nowhere near the elegance of Spring.
And that's exactly why it's not Spring... it's Winter ❄️
```

## ❄️ Ww.Winter - Basic Queries

The goal here is to define a class that contains a `DbContext` and is decorated with a set of attributes at the class level.
Each attribute specifies:
- The __Entity__ it operates on
- The __name of the method__ to generate

The method name itself serves as a __recipe__ - it helps deduce:
- What filters should be applied
- What input parameters are required

### 🧪 Example: Car Entity & Queries

Let's start with an example definition using a `Car` entity and our intended queries class:
- `GetCarByVin` - should return the car with the specified VIN
- `FindCarByModelAndManufacturer` - should return cars filtered by model and/or manufacturer
- If no matching entity is found, a `NotFoundEntityException` should be thrown

### Entity
```
public class Car
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string Manufacturer { get; set; }
    public string Color { get; set; }
    public string Vin { get; set; }
}
```

### Queries class
```
[BasicQuery(typeof(Car), "GetCarByVin")]
[BasicQuery(typeof(Car), "FindCarByModelAndManufacturer")]
public partial class CarQueries
{
    private readonly AppDbContext dbContext;

    public CarQueries(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
}
```

Just by referencing the generator, compilation-time magic happens - and we get the following piece of implementation generated for us:

```
public async Task<Car> FindCarByModelAndManufacturer(
    String? model,
    String? manufacturer,
    CancellationToken cancellationToken)
{
    var query = this.dbContext.Cars
        .AsNoTracking()
        .TagWith("FindCarByModelAndManufacturer");

    if (model is not null)
    {
        query = query.Where(e => e.Model == model);
    }
    if (manufacturer is not null)
    {
        query = query.Where(e => e.Manufacturer == manufacturer);
    }

    var result = await query.SingleOrDefaultAsync(cancellationToken);
    if (result is null)
    {
        throw new NotFoundEntityException("Car", "Id lookup failure");
    }
    return result;
}
```

__Juicy!__

### 🚀 What More Could Be Added?

This is just the basic PoC content - but we can imagine many exciting features growing from it!
- `JPA-style CrudRepository<?, ?> equivalent` - If you implement a well-known interface like this, you could get a full implementation _for free_ - no boilerplate needed!
- `Attribute-driven enhancements` - Using attributes, we could easily add: auditing, projections, eager loading of dependent entities, query customization
- `Smart partial class integration` - We could detect well-known helper methods in a partial class and use them to enhance or override parts of the generated code:
  - Have a method like `IncludeCarDependencies`? Before applying filters, we could modify the query to include related entities using `Include`
  - Have a method like `ProjectTo<CarDTO>`? We could automatically add a `.Select(...)` clause to avoid over-fetching data

```
Alright now, wasn't that fun
Let's try something else
...
(linkin-park,cure-for-the-itch)
```

## ❄️ Ww.Winter - Queries

🔥 One Step Closer to Pandemonium!

Let's take things further and gain more control over our queries.

Instead of relying solely on method name conventions, we can explicitly define method signatures and decorate them with appropriate attributes.

In real-world scenarios, query filters in our HTTP+JSON APIs (often called REST-like) can grow unexpectedly.
So let's:
- Pack filters into structured objects
- Make them optional
- Control naming and behavior
- Add support for sorting and pagination

### 📚 Example: Book API Endpoint

Imagine an API endpoint for books, with several optional query string parameters. These parameters should map to:
- Filter criteria
- Sorting options
- Pagination controls

All of this can be wrapped into a single query object, making the endpoint cleaner and the query logic more maintainable.

### API Endpoint
```
var group = app.MapGroup("api/book")
    .WithTags("Book");

group.MapGet("filter", QueryBookFilter).WithName(nameof(QueryBookFilter));
...
private static async Task<IList<Book>> QueryBookFilter(
    [AsParameters] BookQueries.QueryBooksFilter filterParams,
    [AsParameters] SortParams sortParams,
    [AsParameters] PaginationParams paginationParams,
    [FromServices] BookQueries bookQueries,
    CancellationToken cancellationToken
)
{
    return await bookQueries.QueryBooks(filterParams, sortParams, paginationParams, cancellationToken);
}

```

### Query Parameters

```
public sealed record QueryBooksFilter
{
    public string? TitlePrefix { get; init; }
    public string? TitleFragment { get; init; }
    public string? Isbn { get; init; }
    public string? AuthorFragment { get; init; }
    public string? CategoryFragment { get; init; }
    public int? PageCountFrom { get; init; }
    public int? PageCountTo { get; init; }
}

```

### Query Class
```
public partial class BookQueries
{
    private readonly BookDbContext dbContext;

    public BookQueries(BookDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public sealed record QueryBooksFilter
    {
        public string? TitlePrefix { get; init; }
        public string? TitleFragment { get; init; }
        public string? Isbn { get; init; }
        public string? AuthorFragment { get; init; }
        public string? CategoryFragment { get; init; }
        public int? PageCountFrom { get; init; }
        public int? PageCountTo { get; init; }
    }
    [Query(typeof(Book))]
    public partial Task<IList<Book>> QueryBooks(
        QueryBooksFilter filter,
        SortParams sort,
        PaginationParams pagination,
        CancellationToken cancellationToken
    );
}
```

And that's it!
A generator with just a few hundred lines of code could produce this kind of beauty for you - automatically, effortlessly, and elegantly!

```
public partial async Task<IList<Book>> QueryBooks(
    QueryBooksFilter filter,
    SortParams sort,
    PaginationParams pagination,
    CancellationToken cancellationToken)
{
    var query = this.dbContext.Books
        .AsNoTracking()
        .TagWith("QueryBooks");

    if (filter.TitlePrefix is not null)
    {
        query = query.Where(e => e.Title.StartsWith(filter.TitlePrefix));
    }
    if (filter.TitleFragment is not null)
    {
        query = query.Where(e => e.Title.Contains(filter.TitleFragment));
    }
    if (filter.Isbn is not null)
    {
        query = query.Where(e => e.Isbn == filter.Isbn);
    }
    if (filter.AuthorFragment is not null)
    {
        query = query.Where(e => e.Author.Contains(filter.AuthorFragment));
    }
    if (filter.CategoryFragment is not null)
    {
        query = query.Where(e => e.Category.Contains(filter.CategoryFragment));
    }
    if (filter.PageCountFrom is not null)
    {
        query = query.Where(e => e.PageCount >= filter.PageCountFrom);
    }
    if (filter.PageCountTo is not null)
    {
        query = query.Where(e => e.PageCount <= filter.PageCountTo);
    }

    query = ApplySort(query, sort);
    query = ApplyPagination(query, pagination);

    return await query.ToListAsync(cancellationToken);
}
```

Yummy! All for free! 🙂

Oh, and let's not forget - you also get a few primitive helper methods that you can use inside your generated methods!

### Primitive Methods

```
    private IQueryable<Book> ApplySort(IQueryable<Book> query, SortParams sort)
    {
        if (sort.Properties.Count == 0)
        {
            return query;
        }

        foreach (var property in sort.Properties)
        {
            var propertyName = property.PropertyName.ToLowerInvariant();
            var direction = property.Direction;
            switch (propertyName)
            {
                case "id":
                    query = ApplySort(query, o => o.Id, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "isbn":
                    query = ApplySort(query, o => o.Isbn, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "title":
                    query = ApplySort(query, o => o.Title, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "author":
                    query = ApplySort(query, o => o.Author, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "category":
                    query = ApplySort(query, o => o.Category, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
                case "pagecount":
                    query = ApplySort(query, o => o.PageCount, direction == System.ComponentModel.ListSortDirection.Descending);
                    break;
            }
        }
        return query;
    }

    private IQueryable<Book> ApplyPagination(IQueryable<Book> query, PaginationParams pagination)
    {
        if (pagination.IsDefined)
        {
            return query.Skip(pagination?.Skip ?? 0).Take(pagination?.Take ?? 10);
        }
        return query;
    }

```


### 🚀 What More Could Be Added?

What else could we push further? Plenty!
- Since we generate primitives like `ApplySort` or `ApplyPagination`, we can reuse them to craft custom methods with minimal effort.
- Just like in __Basic Queries__, we could define a set of well-known extension points. When detected during generation, they could be automatically pulled in and used!
- And of course, we can handle tons of other scenarios directly in the generator: better diagnostics, smarter defaults, more flexible query composition, etc

Thanks, JPA Repositories - for the inspiration!
👉 [GitHub - Spring Data JPA](https://github.com/spring-projects/spring-data-jpa)
