using Microsoft.EntityFrameworkCore;

namespace Ww.Winter.Some.Books;

public sealed class BookDbContext
{
    public required DbSet<Book> Books { get; set; }
}
