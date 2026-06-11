using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/trips")]
public class TripsApiController : ApiControllerBase
{
    public TripsApiController(AppDbContext context) : base(context) { }

    private IQueryable<Trip> DetailedQuery() =>
        Context.Trips
            .AsNoTracking()
            .Include(trip => trip.User)
            .Include(trip => trip.Destinations.Where(destination => !destination.IsDeleted))
                .ThenInclude(destination => destination.Activities.Where(activity => !activity.IsDeleted))
            .Include(trip => trip.Destinations.Where(destination => !destination.IsDeleted))
                .ThenInclude(destination => destination.Accommodations.Where(accommodation => !accommodation.IsDeleted))
            .Include(trip => trip.Destinations.Where(destination => !destination.IsDeleted))
                .ThenInclude(destination => destination.Transports.Where(transport => !transport.IsDeleted))
            .Include(trip => trip.Destinations.Where(destination => !destination.IsDeleted))
                .ThenInclude(destination => destination.Reviews.Where(review => !review.IsDeleted))
                    .ThenInclude(review => review.User);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? userId,
        [FromQuery] DateTime? startsFrom,
        [FromQuery] DateTime? endsBy)
    {
        var query = DetailedQuery().Where(trip => !trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(trip =>
                trip.Name.ToLower().Contains(search) ||
                (trip.User.FirstName + " " + trip.User.LastName).ToLower().Contains(search));
        }

        if (userId.HasValue) query = query.Where(trip => trip.UserId == userId);
        if (startsFrom.HasValue) query = query.Where(trip => trip.StartDate >= startsFrom);
        if (endsBy.HasValue) query = query.Where(trip => trip.EndDate <= endsBy);

        var trips = await query.OrderBy(trip => trip.StartDate).ThenBy(trip => trip.Name).ToListAsync();
        return Ok(trips.Select(trip => trip.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TripDto>> GetById(int id)
    {
        var trip = await DetailedQuery().FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        return trip == null ? NotFound() : Ok(trip.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TripDto>> Create(TripCreateDto request)
    {
        if (request.StartDate >= request.EndDate)
        {
            ModelState.AddModelError(nameof(request.EndDate), "End date must be after the start date.");
            return ValidationProblem(ModelState);
        }

        if (!await Context.Users.AnyAsync(user => user.Id == request.UserId))
        {
            ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
            return ValidationProblem(ModelState);
        }

        var trip = new Trip
        {
            Name = request.Name.Trim(),
            StartDate = request.StartDate!.Value,
            EndDate = request.EndDate!.Value,
            UserId = request.UserId!.Value
        };

        Context.Trips.Add(trip);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = trip.Id }, (await DetailedQuery().FirstAsync(item => item.Id == trip.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TripDto>> Update(int id, TripUpdateDto request)
    {
        var trip = await Context.Trips.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (trip == null) return NotFound();

        if (request.StartDate >= request.EndDate)
        {
            ModelState.AddModelError(nameof(request.EndDate), "End date must be after the start date.");
            return ValidationProblem(ModelState);
        }

        if (!await Context.Users.AnyAsync(user => user.Id == request.UserId))
        {
            ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
            return ValidationProblem(ModelState);
        }

        var rangeStart = request.StartDate!.Value.Date;
        var rangeEnd = request.EndDate!.Value.Date.AddDays(1).AddTicks(-1);
        var childDatesValid = !await Context.Destinations
            .Where(destination => destination.TripId == id && !destination.IsDeleted)
            .AnyAsync(destination =>
                destination.Activities.Any(activity => !activity.IsDeleted && (activity.Date < rangeStart || activity.Date > rangeEnd)) ||
                destination.Accommodations.Any(accommodation => !accommodation.IsDeleted && (accommodation.CheckInDate < rangeStart || accommodation.CheckOutDate > rangeEnd)) ||
                destination.Transports.Any(transport => !transport.IsDeleted && (transport.DepartureTime < rangeStart || transport.ArrivalTime > rangeEnd)));

        if (!childDatesValid)
        {
            ModelState.AddModelError(nameof(request.EndDate), "The new trip range would exclude existing itinerary items.");
            return ValidationProblem(ModelState);
        }

        trip.Name = request.Name.Trim();
        trip.StartDate = request.StartDate!.Value;
        trip.EndDate = request.EndDate!.Value;
        trip.UserId = request.UserId!.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await Context.Trips.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (trip == null) return NotFound();

        trip.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }
}
