using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using projektLana.Api.Dtos;
using Xunit;

namespace projektLana.Tests;

public class ReviewsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReviewsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndReviews()
    {
        var (trip, destination) = CreateGraph();
        destination.Reviews.Add(CreateReview("First review"));
        destination.Reviews.Add(CreateReview("Second review"));
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync("/api/reviews");
        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(reviews);
        Assert.Equal(2, reviews.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkForExistingReview()
    {
        var (trip, destination) = CreateGraph();
        var review = CreateReview();
        destination.Reviews.Add(review);
        await _factory.SeedAsync(trip);

        var response = await _client.GetAsync($"/api/reviews/{review.Id}");
        var result = await response.Content.ReadFromJsonAsync<ReviewDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(review.Id, result.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForMissingReview()
    {
        await _factory.SeedAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/reviews/999999")).StatusCode);
    }

    [Fact]
    public async Task Post_CreatesReviewForValidRequest()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = new ReviewCreateDto
        {
            Rating = 5,
            Comment = "Excellent destination",
            UserId = await GetUserIdAsync(),
            DestinationId = destination.Id
        };

        var response = await _client.PostAsJsonAsync("/api/reviews", request);
        var created = await response.Content.ReadFromJsonAsync<ReviewDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(request.Rating, created.Rating);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Reviews.AnyAsync(item => item.Id == created.Id && item.DestinationId == destination.Id)));
    }

    [Fact]
    public async Task Post_ReturnsBadRequestForInvalidData()
    {
        var (trip, destination) = CreateGraph();
        await _factory.SeedAsync(trip);
        var request = new ReviewCreateDto
        {
            Rating = 6,
            Comment = "Invalid rating",
            UserId = await GetUserIdAsync(),
            DestinationId = destination.Id
        };

        var response = await _client.PostAsJsonAsync("/api/reviews", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await _factory.WithDbContextAsync(context => context.Reviews.CountAsync()));
    }

    [Fact]
    public async Task Put_UpdatesExistingReview()
    {
        var (trip, destination) = CreateGraph();
        var review = CreateReview();
        destination.Reviews.Add(review);
        await _factory.SeedAsync(trip);
        var request = new ReviewUpdateDto
        {
            Rating = 4,
            Comment = "Updated review",
            UserId = await GetUserIdAsync(),
            DestinationId = destination.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/reviews/{review.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<ReviewDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Updated review", updated.Comment);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Reviews.AnyAsync(item => item.Id == review.Id && item.Rating == 4)));
    }

    [Fact]
    public async Task Delete_RemovesExistingReview()
    {
        var (trip, destination) = CreateGraph();
        var review = CreateReview();
        destination.Reviews.Add(review);
        await _factory.SeedAsync(trip);

        var response = await _client.DeleteAsync($"/api/reviews/{review.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _factory.WithDbContextAsync(context =>
            context.Reviews.AnyAsync(item => item.Id == review.Id && item.IsDeleted)));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/reviews/{review.Id}")).StatusCode);
    }

    private async Task<int> GetUserIdAsync() =>
        await _factory.WithDbContextAsync(context => context.Users.Select(user => user.Id).SingleAsync());

    private static (Trip Trip, Destination Destination) CreateGraph()
    {
        var trip = IntegrationTestData.CreateTrip();
        return (trip, IntegrationTestData.AddDestination(trip));
    }

    private static Review CreateReview(string comment = "Great destination") =>
        new()
        {
            Rating = 5,
            Comment = comment
        };
}
