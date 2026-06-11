using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class DestinationsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DestinationsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndDestinations()
    {
        var trip = IntegrationTestData.CreateTrip();
        IntegrationTestData.AddDestination(trip, "Zagreb");
        IntegrationTestData.AddDestination(trip, "Split");
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync("/api/destinations");
        var destinations = await response.Content.ReadFromJsonAsync<List<DestinationDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingDestination()
    {
        var trip = IntegrationTestData.CreateTrip();
        var destination = IntegrationTestData.AddDestination(trip);
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/destinations/{destination.Id}");
        var result = await response.Content.ReadFromJsonAsync<DestinationDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(destination.Id, result.Id);
        Assert.Equal("Zagreb", result.City);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingDestination()
    {
        await _factory.SeedAsync();

        var response = await _client.GetAsync("/api/destinations/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesDestinationForValidRequest()
    {
        var trip = IntegrationTestData.CreateTrip();
        await _factory.SeedAsync(trip);
        var request = new DestinationCreateDto
        {
            City = "Dubrovnik",
            Country = "Croatia",
            Description = "Old town",
            TripId = trip.Id
        };

        var response = await _client.PostAsJsonAsync("/api/destinations", request);
        var created = await response.Content.ReadFromJsonAsync<DestinationDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Dubrovnik", created.City);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Destinations.AnyAsync(item => item.Id == created.Id && item.TripId == trip.Id)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        await _factory.SeedAsync();
        var request = new DestinationCreateDto
        {
            City = "Dubrovnik",
            Country = "Croatia",
            Description = "Old town",
            TripId = 999999
        };

        var response = await _client.PostAsJsonAsync("/api/destinations", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Destinations.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingDestination()
    {
        var trip = IntegrationTestData.CreateTrip();
        var destination = IntegrationTestData.AddDestination(trip);
        await _factory.SeedAsync(trip);
        var request = new DestinationUpdateDto
        {
            City = "Rijeka",
            Country = "Croatia",
            Description = "Updated destination",
            TripId = trip.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/destinations/{destination.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<DestinationDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Rijeka", updated.City);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Destinations.AnyAsync(item => item.Id == destination.Id && item.City == "Rijeka")));
    }

    [Fact]
    public async Task Delete_RemovesExistingDestination()
    {
        var trip = IntegrationTestData.CreateTrip();
        var destination = IntegrationTestData.AddDestination(trip);
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/destinations/{destination.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Destinations.AnyAsync(item => item.Id == destination.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/destinations/{destination.Id}")).StatusCode);
    }
}
