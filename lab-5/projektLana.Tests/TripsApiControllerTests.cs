using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class TripsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TripsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndTrips()
    {
        await _factory.SeedAsync(CreateTrip("First Trip"), CreateTrip("Second Trip"));

        var response = await _client.GetAsync("/api/trips");
        var trips = await response.Content.ReadFromJsonAsync<List<TripDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(trips);
        Assert.Equal(2, trips.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingTrip()
    {
        var trip = CreateTrip("Existing Trip");
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/trips/{trip.Id}");
        var result = await response.Content.ReadFromJsonAsync<TripDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(trip.Id, result.Id);
        Assert.Equal("Existing Trip", result.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingTrip()
    {
        await _factory.SeedAsync();

        var response = await _client.GetAsync("/api/trips/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesTripForValidRequest()
    {
        await _factory.SeedAsync();
        var userId = await GetUserIdAsync();
        var request = new TripCreateDto
        {
            Name = "New API Trip",
            StartDate = new DateTime(2027, 5, 1),
            EndDate = new DateTime(2027, 5, 10),
            UserId = userId
        };

        var response = await _client.PostAsJsonAsync("/api/trips", request);
        var created = await response.Content.ReadFromJsonAsync<TripDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.NotNull(response.Headers.Location);
        Assert.True(await _factory.WithDbContextAsync(
            context => context.Trips.AnyAsync(trip => trip.Id == created.Id && trip.Name == request.Name)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        await _factory.SeedAsync();
        var request = new TripCreateDto
        {
            Name = "No",
            StartDate = new DateTime(2027, 5, 10),
            EndDate = new DateTime(2027, 5, 1),
            UserId = await GetUserIdAsync()
        };

        var response = await _client.PostAsJsonAsync("/api/trips", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Trips.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingTrip()
    {
        var trip = CreateTrip("Original Trip");
        await _factory.SeedAsync(trip);
        var request = new TripUpdateDto
        {
            Name = "Updated Trip",
            StartDate = new DateTime(2027, 8, 1),
            EndDate = new DateTime(2027, 8, 20),
            UserId = await GetUserIdAsync()
        };

        var response = await _client.PutAsJsonAsync($"/api/trips/{trip.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<TripDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Updated Trip", updated.Name);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Trips.AnyAsync(item =>
                item.Id == trip.Id &&
                item.Name == "Updated Trip" &&
                item.StartDate == request.StartDate &&
                item.EndDate == request.EndDate)));
    }

    [Fact]
    public async Task Delete_RemovesExistingTrip()
    {
        var trip = CreateTrip("Trip To Delete");
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/trips/{trip.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Trips.AnyAsync(item => item.Id == trip.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/trips/{trip.Id}")).StatusCode);
    }

    private async Task<int> GetUserIdAsync() =>
        await _factory.WithDbContextAsync(context => context.Users.Select(user => user.Id).SingleAsync());

    private static Trip CreateTrip(string name) =>
        new()
        {
            Name = name,
            StartDate = new DateTime(2027, 6, 1),
            EndDate = new DateTime(2027, 6, 15)
        };
}
