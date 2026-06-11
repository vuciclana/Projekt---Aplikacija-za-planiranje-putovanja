using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class AccommodationsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AccommodationsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndAccommodations()
    {
        var (trip, destination) = CreateGraph();
        destination.Accommodations.Add(CreateAccommodation("First Hotel"));
        destination.Accommodations.Add(CreateAccommodation("Second Hotel"));
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync("/api/accommodations");
        var accommodations = await response.Content.ReadFromJsonAsync<List<AccommodationDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(accommodations);
        Assert.Equal(2, accommodations.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingAccommodation()
    {
        var (trip, destination) = CreateGraph();
        var accommodation = CreateAccommodation();
        destination.Accommodations.Add(accommodation);
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/accommodations/{accommodation.Id}");
        var result = await response.Content.ReadFromJsonAsync<AccommodationDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(accommodation.Id, result.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingAccommodation()
    {
        await _factory.SeedAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/accommodations/999999")).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesAccommodationForValidRequest()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id, "New Hotel");

        var response = await _client.PostAsJsonAsync("/api/accommodations", request);
        var created = await response.Content.ReadFromJsonAsync<AccommodationDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Accommodations.AnyAsync(item => item.Id == created.Id && item.DestinationId == destination.Id)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id);
        request.CheckOutDate = new DateTime(2027, 6, 3);

        var response = await _client.PostAsJsonAsync("/api/accommodations", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Accommodations.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingAccommodation()
    {
        var (trip, destination) = CreateGraph();
        var accommodation = CreateAccommodation();
        destination.Accommodations.Add(accommodation);
        await _factory.SeedAsync(trip);
        var request = new AccommodationUpdateDto
        {
            Name = "Updated Hotel",
            Type = AccommodationType.Resort,
            Address = "Updated Address 10",
            CostPerNight = 150m,
            CheckInDate = new DateTime(2027, 6, 6),
            CheckOutDate = new DateTime(2027, 6, 10),
            DestinationId = destination.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/accommodations/{accommodation.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<AccommodationDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Updated Hotel", updated.Name);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Accommodations.AnyAsync(item => item.Id == accommodation.Id && item.CostPerNight == 150m)));
    }

    [Fact]
    public async Task Delete_RemovesExistingAccommodation()
    {
        var (trip, destination) = CreateGraph();
        var accommodation = CreateAccommodation();
        destination.Accommodations.Add(accommodation);
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/accommodations/{accommodation.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Accommodations.AnyAsync(item => item.Id == accommodation.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/accommodations/{accommodation.Id}")).StatusCode);
    }

    private static (Trip Trip, Destination Destination) CreateGraph()
    {
        var trip = IntegrationTestData.CreateTrip();
        return (trip, IntegrationTestData.AddDestination(trip));
    }

    private static Accommodation CreateAccommodation(string name = "Test Hotel") =>
        new()
        {
            Name = name,
            Type = AccommodationType.Hotel,
            Address = "Test Street 1",
            CostPerNight = 100m,
            CheckInDate = new DateTime(2027, 6, 4),
            CheckOutDate = new DateTime(2027, 6, 8)
        };

    private static AccommodationCreateDto CreateRequest(int destinationId, string name = "Test Hotel") =>
        new()
        {
            Name = name,
            Type = AccommodationType.Hotel,
            Address = "Test Street 1",
            CostPerNight = 100m,
            CheckInDate = new DateTime(2027, 6, 4),
            CheckOutDate = new DateTime(2027, 6, 8),
            DestinationId = destinationId
        };
}
