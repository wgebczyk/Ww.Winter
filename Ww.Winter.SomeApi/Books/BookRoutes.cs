using Microsoft.AspNetCore.Mvc;

namespace Ww.Winter.Some.Books;

public static class BookRoutes
{
    public static void MapBookRoutes(this WebApplication app)
    {
        var group = app.MapGroup("api/book")
            .WithTags("Book");

        group.MapGet("isbn/{isbn}", QueryBookByIsbn).WithName(nameof(QueryBookByIsbn));
        group.MapGet("title/{title}", QueryBookByTitle).WithName(nameof(QueryBookByTitle));
        group.MapGet("filter", QueryBookFilter).WithName(nameof(QueryBookFilter));
    }

    private static async Task<Book> QueryBookByIsbn(
        [FromRoute] string isbn,
        [FromServices] BookBasicQueries bookQueries,
        CancellationToken cancellationToken
    )
    {
        return await bookQueries.GetBookByIsbn(isbn, cancellationToken);
    }
    private static async Task<Book> QueryBookByTitle(
        [FromRoute] string title,
        [FromServices] BookBasicQueries bookQueries,
        CancellationToken cancellationToken
    )
    {
        return await bookQueries.GetBookByTitle(title, cancellationToken);
    }

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
}
