using Ww.Winter.Some.Books;
using Ww.Winter.Some.Cars;
using Ww.Winter.Some.Packages;

namespace Ww.Winter.SomeApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        app.MapBookRoutes();
        app.MapCarRoutes();
        app.MapPackageRoutes();

        app.Run();
    }
}
