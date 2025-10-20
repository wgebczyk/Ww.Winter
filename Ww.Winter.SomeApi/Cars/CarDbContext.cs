using Microsoft.EntityFrameworkCore;

namespace Ww.Winter.Some.Cars;

public sealed class CarDbContext
{
    public required DbSet<Car> Cars { get; set; }
}
