using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class TransportsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TransportsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndTransports()
    {
        var (trip, destination) = CreateGraph();
        destination.Transports.Add(CreateTransport(TransportType.Bus));
        destination.Transports.Add(CreateTransport(TransportType.Train));
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync("/api/transports");
        var transports = await response.Content.ReadFromJsonAsync<List<TransportDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(transports);
        Assert.Equal(2, transports.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingTransport()
    {
        var (trip, destination) = CreateGraph();
        var transport = CreateTransport();
        destination.Transports.Add(transport);
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/transports/{transport.Id}");
        var result = await response.Content.ReadFromJsonAsync<TransportDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(transport.Id, result.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingTransport()
    {
        await _factory.SeedAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/transports/999999")).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesTransportForValidRequest()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id);

        var response = await _client.PostAsJsonAsync("/api/transports", request);
        var created = await response.Content.ReadFromJsonAsync<TransportDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(request.Type, created.Type);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Transports.AnyAsync(item => item.Id == created.Id && item.DestinationId == destination.Id)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id);
        request.ArrivalTime = new DateTime(2027, 6, 5, 7, 0, 0);

        var response = await _client.PostAsJsonAsync("/api/transports", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Transports.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingTransport()
    {
        var (trip, destination) = CreateGraph();
        var transport = CreateTransport();
        destination.Transports.Add(transport);
        await _factory.SeedAsync(trip);
        var request = new TransportUpdateDto
        {
            Type = TransportType.Train,
            Cost = 80m,
            DepartureTime = new DateTime(2027, 6, 10, 9, 0, 0),
            ArrivalTime = new DateTime(2027, 6, 10, 12, 0, 0),
            DestinationId = destination.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/transports/{transport.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<TransportDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(TransportType.Train, updated.Type);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Transports.AnyAsync(item => item.Id == transport.Id && item.Cost == 80m)));
    }

    [Fact]
    public async Task Delete_RemovesExistingTransport()
    {
        var (trip, destination) = CreateGraph();
        var transport = CreateTransport();
        destination.Transports.Add(transport);
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/transports/{transport.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Transports.AnyAsync(item => item.Id == transport.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/transports/{transport.Id}")).StatusCode);
    }

    private static (Trip Trip, Destination Destination) CreateGraph()
    {
        var trip = IntegrationTestData.CreateTrip();
        return (trip, IntegrationTestData.AddDestination(trip));
    }

    private static Transport CreateTransport(TransportType type = TransportType.Bus) =>
        new()
        {
            Type = type,
            Cost = 40m,
            DepartureTime = new DateTime(2027, 6, 5, 8, 0, 0),
            ArrivalTime = new DateTime(2027, 6, 5, 10, 0, 0)
        };

    private static TransportCreateDto CreateRequest(int destinationId) =>
        new()
        {
            Type = TransportType.Bus,
            Cost = 40m,
            DepartureTime = new DateTime(2027, 6, 5, 8, 0, 0),
            ArrivalTime = new DateTime(2027, 6, 5, 10, 0, 0),
            DestinationId = destinationId
        };
}
