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
    }
    //[Query(typeof(Book))]
    //public partial Task<IList<Book>> QueryBooks(
    //    QueryBooksFilter filter,
    //    SortParams sort,
    //    PaginationParams pagination,
    //    CancellationToken cancellationToken
    //);
}
