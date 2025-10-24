namespace Ww.Winter.Some.Books;

[BasicQuery(typeof(Book), "GetBookByIsbn")]
[BasicQuery(typeof(Book), "GetBookByTitle")]
public partial class BookBasicQueries
{
    private readonly BookDbContext dbContext;

    public BookBasicQueries(BookDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
}
