namespace Ww.Winter.Some;

public class Package
{
    public required int Id { get; set; }
    public required string Number { get; set; }

    public required string SenderName { get; set; }
    public required string SenderAddress { get; set; }
    public required string SenderPostalCode { get; set; }

    public required string RecipientName { get; set; }
    public required string RecipientAddress { get; set; }
    public required string RecipientPostalCode { get; set; }
}
