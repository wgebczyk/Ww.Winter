namespace Ww.Winter.Some.Books;

public class Book
{
    public required int Id { get; set; }
    public required string Isbn { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Category { get; set; }
    public required int PageCount { get; set; }
}
