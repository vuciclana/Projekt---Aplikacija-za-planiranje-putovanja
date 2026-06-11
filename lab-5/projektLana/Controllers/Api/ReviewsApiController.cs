using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/reviews")]
public class ReviewsApiController : ApiControllerBase
{
    public ReviewsApiController(AppDbContext context) : base(context) { }

    private IQueryable<Review> DetailedQuery() =>
        Context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .Include(review => review.Destination)
                .ThenInclude(destination => destination.Trip);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? destinationId,
        [FromQuery] int? userId,
        [FromQuery] int? minRating,
        [FromQuery] int? maxRating)
    {
        var query = DetailedQuery().Where(review =>
            !review.IsDeleted &&
            !review.Destination.IsDeleted &&
            !review.Destination.Trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(review =>
                (review.Comment != null && review.Comment.ToLower().Contains(search)) ||
                (review.User.FirstName + " " + review.User.LastName).ToLower().Contains(search) ||
                review.Destination.City.ToLower().Contains(search) ||
                review.Destination.Country.ToLower().Contains(search));
        }

        if (destinationId.HasValue) query = query.Where(review => review.DestinationId == destinationId);
        if (userId.HasValue) query = query.Where(review => review.UserId == userId);
        if (minRating.HasValue) query = query.Where(review => review.Rating >= minRating);
        if (maxRating.HasValue) query = query.Where(review => review.Rating <= maxRating);

        var reviews = await query.OrderByDescending(review => review.Id).ToListAsync();
        return Ok(reviews.Select(review => review.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        var review = await DetailedQuery().FirstOrDefaultAsync(item =>
            item.Id == id && !item.IsDeleted && !item.Destination.IsDeleted && !item.Destination.Trip.IsDeleted);
        return review == null ? NotFound() : Ok(review.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(ReviewCreateDto request)
    {
        if (!await ValidateReferences(request)) return ValidationProblem(ModelState);

        var review = new Review
        {
            Rating = request.Rating!.Value,
            Comment = request.Comment?.Trim(),
            UserId = request.UserId!.Value,
            DestinationId = request.DestinationId!.Value
        };

        Context.Reviews.Add(review);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = review.Id }, (await DetailedQuery().FirstAsync(item => item.Id == review.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReviewDto>> Update(int id, ReviewUpdateDto request)
    {
        var review = await Context.Reviews.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (review == null) return NotFound();
        if (!await ValidateReferences(request)) return ValidationProblem(ModelState);

        review.Rating = request.Rating!.Value;
        review.Comment = request.Comment?.Trim();
        review.UserId = request.UserId!.Value;
        review.DestinationId = request.DestinationId!.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await Context.Reviews.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (review == null) return NotFound();

        review.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ValidateReferences(ReviewCreateDto request)
    {
        if (!await Context.Users.AnyAsync(user => user.Id == request.UserId))
        {
            ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
        }

        if (await GetActiveDestinationWithTrip(request.DestinationId!.Value) == null)
        {
            ModelState.AddModelError(nameof(request.DestinationId), "Active destination does not exist.");
        }

        return ModelState.IsValid;
    }
}
