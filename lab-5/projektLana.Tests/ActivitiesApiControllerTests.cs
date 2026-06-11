using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class ActivitiesApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ActivitiesApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndActivities()
    {
        var (trip, destination) = CreateGraph();
        destination.Activities.Add(CreateActivity("Museum"));
        destination.Activities.Add(CreateActivity("Walking Tour"));
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync("/api/activities");
        var activities = await response.Content.ReadFromJsonAsync<List<ActivityDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(activities);
        Assert.Equal(2, activities.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingActivity()
    {
        var (trip, destination) = CreateGraph();
        var activity = CreateActivity();
        destination.Activities.Add(activity);
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/activities/{activity.Id}");
        var result = await response.Content.ReadFromJsonAsync<ActivityDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(activity.Id, result.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingActivity()
    {
        await _factory.SeedAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/activities/999999")).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesActivityForValidRequest()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id, "New Activity");

        var response = await _client.PostAsJsonAsync("/api/activities", request);
        var created = await response.Content.ReadFromJsonAsync<ActivityDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Activities.AnyAsync(item => item.Id == created.Id && item.DestinationId == destination.Id)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = CreateRequest(destination.Id);
        request.Date = new DateTime(2027, 7, 1);

        var response = await _client.PostAsJsonAsync("/api/activities", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Activities.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingActivity()
    {
        var (trip, destination) = CreateGraph();
        var activity = CreateActivity();
        destination.Activities.Add(activity);
        await _factory.SeedAsync(trip);
        var request = new ActivityUpdateDto
        {
            Name = "Updated Activity",
            Type = ActivityType.Adventure,
            Date = new DateTime(2027, 6, 8),
            Cost = 75m,
            DestinationId = destination.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/activities/{activity.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<ActivityDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Updated Activity", updated.Name);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Activities.AnyAsync(item => item.Id == activity.Id && item.Cost == 75m)));
    }

    [Fact]
    public async Task Delete_RemovesExistingActivity()
    {
        var (trip, destination) = CreateGraph();
        var activity = CreateActivity();
        destination.Activities.Add(activity);
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/activities/{activity.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Activities.AnyAsync(item => item.Id == activity.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/activities/{activity.Id}")).StatusCode);
    }

    private static (Trip Trip, Destination Destination) CreateGraph()
    {
        var trip = IntegrationTestData.CreateTrip();
        return (trip, IntegrationTestData.AddDestination(trip));
    }

    private static Activity CreateActivity(string name = "City Tour") =>
        new()
        {
            Name = name,
            TypeOfActivity = ActivityType.Sightseeing,
            Date = new DateTime(2027, 6, 5),
            Cost = 25m
        };

    private static ActivityCreateDto CreateRequest(int destinationId, string name = "City Tour") =>
        new()
        {
            Name = name,
            Type = ActivityType.Sightseeing,
            Date = new DateTime(2027, 6, 5),
            Cost = 25m,
            DestinationId = destinationId
        };
}
