using Microsoft.EntityFrameworkCore;

namespace Ww.Winter.SomeApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var carQueries = new CarQueries(new AppDbContext { Cars = null! });
        var r0 = await carQueries.FindCarByModelAndManufacturer(model: "X33", manufacturer: null, CancellationToken.None);
        var r1 = await carQueries.GetCarByVin(vin: "D3322330000111", CancellationToken.None);
    }
}

public sealed class AppDbContext
{
    public required DbSet<Car> Cars { get; set; }
}

public class Car
{
    public required int Id { get; set; }
    public required string Model { get; set; }
    public required string Manufacturer { get; set; }
    public required string Color { get; set; }
    public required string Vin { get; set; }
}

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
