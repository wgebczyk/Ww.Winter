namespace Ww.Winter.Some.Cars;

public partial class CarQueries
{
    private readonly CarDbContext dbContext;

    public CarQueries(CarDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [QueryableFilter(typeof(Car))]
    public sealed partial record QueryCarsFilter
    {
        public string? ManufacturerPrefix { get; init; }
        public string? ManufacturerFragment { get; init; }
        public string? ModelPrefix { get; init; }
        public int? Year { get; init; }
        public int? YearFrom { get; init; }
        public int? YearTo { get; init; }
        public string? Vin { get; init; }
        public string? VinPrefix { get; init; }
    }
    [Query(typeof(Car))]
    public partial Task<IList<Car>> QueryCars(
        QueryCarsFilter filter,
        SortParams sort,
        PaginationParams pagination,
        CancellationToken cancellationToken
    );
}
