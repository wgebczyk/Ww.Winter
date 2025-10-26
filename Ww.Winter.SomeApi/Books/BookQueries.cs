using Microsoft.EntityFrameworkCore;

namespace Ww.Winter.Some.Books;

public partial class BookQueries
{
    private readonly BookDbContext dbContext;

    public BookQueries(BookDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [QueryableFilter(typeof(Book))]
    public partial class QueryBooksFilter
    {
        public string? TitlePrefix { get; init; }
        public string? TitleFragment { get; init; }
        public string? Isbn { get; init; }
        public string? AuthorFragment { get; init; }
        public string? CategoryFragment { get; init; }
        public int? PageCountFrom { get; init; }
        public int? PageCountTo { get; init; }
        public bool OnlySpecialFlag { get; init; }
    }
    [Query(typeof(Book))]
    public partial Task<IList<Book>> QueryBooks(
        QueryBooksFilter filter,
        SortParams sort,
        PaginationParams pagination,
        CancellationToken cancellationToken
    );
    public sealed record BookSummaryDto(
        string Title,
        string Isbn,
        string AuthorName
    );

    private IQueryable<Book> GetBaseQuery()
    {
        return dbContext.Books.IgnoreQueryFilters();
    }

    //public BookSummaryDto QueryBooksProjectTo(Book entity) {
    //    return new BookSummaryDto(
    //        entity.Title,
    //        entity.Isbn,
    //        entity.Author
    //    );
    //}

    private static IQueryable<Book> ApplyOnlySpecialFlag(IQueryable<Book> query, bool value)
    {
        if (!value)
        {
            return query;
        }
        return query.Where(b => b.Category == "Special");
    }
}
