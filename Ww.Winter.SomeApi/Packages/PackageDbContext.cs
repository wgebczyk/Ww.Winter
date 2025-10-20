using Microsoft.EntityFrameworkCore;

namespace Ww.Winter.Some.Packages;

public sealed class PackageDbContext
{
    public required DbSet<Package> Packages { get; set; }
}
