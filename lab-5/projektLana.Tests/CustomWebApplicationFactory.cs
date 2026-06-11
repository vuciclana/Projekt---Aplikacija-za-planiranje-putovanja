using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using projektLana.Data;

namespace projektLana.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"projektLana-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task SeedAsync(params Trip[] trips)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Reviews.RemoveRange(context.Reviews);
        context.Activities.RemoveRange(context.Activities);
        context.Accommodations.RemoveRange(context.Accommodations);
        context.Transports.RemoveRange(context.Transports);
        context.Destinations.RemoveRange(context.Destinations);
        context.Trips.RemoveRange(context.Trips);
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Test",
            LastName = "Traveler",
            Email = "traveler@example.com"
        };

        context.Users.Add(user);
        foreach (var trip in trips)
        {
            trip.User = user;
            foreach (var review in trip.Destinations.SelectMany(destination => destination.Reviews))
            {
                review.User = user;
            }

            context.Trips.Add(trip);
        }

        await context.SaveChangesAsync();
    }

    public async Task<TResult> WithDbContextAsync<TResult>(Func<AppDbContext, Task<TResult>> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await action(context);
    }
}
