using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/destinations")]
public class DestinationsApiController : ApiControllerBase
{
    public DestinationsApiController(AppDbContext context) : base(context) { }

    private IQueryable<Destination> DetailedQuery() =>
        Context.Destinations
            .AsNoTracking()
            .Include(destination => destination.Trip)
            .Include(destination => destination.Activities.Where(activity => !activity.IsDeleted))
            .Include(destination => destination.Accommodations.Where(accommodation => !accommodation.IsDeleted))
            .Include(destination => destination.Transports.Where(transport => !transport.IsDeleted))
            .Include(destination => destination.Reviews.Where(review => !review.IsDeleted))
                .ThenInclude(review => review.User);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DestinationDto>>> GetAll([FromQuery] string? q, [FromQuery] int? tripId)
    {
        var query = DetailedQuery().Where(destination => !destination.IsDeleted && !destination.Trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(destination =>
                destination.City.ToLower().Contains(search) ||
                destination.Country.ToLower().Contains(search) ||
                destination.Description.ToLower().Contains(search) ||
                destination.Trip.Name.ToLower().Contains(search));
        }

        if (tripId.HasValue) query = query.Where(destination => destination.TripId == tripId);

        var destinations = await query.OrderBy(destination => destination.Country).ThenBy(destination => destination.City).ToListAsync();
        return Ok(destinations.Select(destination => destination.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DestinationDto>> GetById(int id)
    {
        var destination = await DetailedQuery()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted && !item.Trip.IsDeleted);
        return destination == null ? NotFound() : Ok(destination.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<DestinationDto>> Create(DestinationCreateDto request)
    {
        if (!await Context.Trips.AnyAsync(trip => trip.Id == request.TripId && !trip.IsDeleted))
        {
            ModelState.AddModelError(nameof(request.TripId), "Active trip does not exist.");
            return ValidationProblem(ModelState);
        }

        var destination = new Destination
        {
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            Description = request.Description.Trim(),
            TripId = request.TripId!.Value
        };

        Context.Destinations.Add(destination);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = destination.Id }, (await DetailedQuery().FirstAsync(item => item.Id == destination.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DestinationDto>> Update(int id, DestinationUpdateDto request)
    {
        var destination = await Context.Destinations.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (destination == null) return NotFound();

        var trip = await Context.Trips.FirstOrDefaultAsync(item => item.Id == request.TripId && !item.IsDeleted);
        if (trip == null)
        {
            ModelState.AddModelError(nameof(request.TripId), "Active trip does not exist.");
            return ValidationProblem(ModelState);
        }

        var rangeStart = trip.StartDate.Date;
        var rangeEnd = trip.EndDate.Date.AddDays(1).AddTicks(-1);
        var childDatesValid = !await Context.Destinations
            .Where(item => item.Id == id)
            .AnyAsync(item =>
                item.Activities.Any(activity => !activity.IsDeleted && (activity.Date < rangeStart || activity.Date > rangeEnd)) ||
                item.Accommodations.Any(accommodation => !accommodation.IsDeleted && (accommodation.CheckInDate < rangeStart || accommodation.CheckOutDate > rangeEnd)) ||
                item.Transports.Any(transport => !transport.IsDeleted && (transport.DepartureTime < rangeStart || transport.ArrivalTime > rangeEnd)));

        if (!childDatesValid)
        {
            ModelState.AddModelError(nameof(request.TripId), "The selected trip range does not contain existing itinerary items.");
            return ValidationProblem(ModelState);
        }

        destination.City = request.City.Trim();
        destination.Country = request.Country.Trim();
        destination.Description = request.Description.Trim();
        destination.TripId = request.TripId!.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var destination = await Context.Destinations.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (destination == null) return NotFound();

        destination.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }
}
