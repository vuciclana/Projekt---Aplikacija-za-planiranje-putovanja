using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/accommodations")]
public class AccommodationsApiController : ApiControllerBase
{
    public AccommodationsApiController(AppDbContext context) : base(context) { }

    private IQueryable<Accommodation> DetailedQuery() =>
        Context.Accommodations
            .AsNoTracking()
            .Include(accommodation => accommodation.Destination)
                .ThenInclude(destination => destination.Trip);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccommodationDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? destinationId,
        [FromQuery] AccommodationType? type,
        [FromQuery] DateTime? availableFrom,
        [FromQuery] DateTime? availableTo)
    {
        var query = DetailedQuery().Where(accommodation =>
            !accommodation.IsDeleted &&
            !accommodation.Destination.IsDeleted &&
            !accommodation.Destination.Trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(accommodation =>
                accommodation.Name.ToLower().Contains(search) ||
                accommodation.Address.ToLower().Contains(search) ||
                accommodation.Destination.City.ToLower().Contains(search) ||
                accommodation.Destination.Country.ToLower().Contains(search));
        }

        if (destinationId.HasValue) query = query.Where(accommodation => accommodation.DestinationId == destinationId);
        if (type.HasValue) query = query.Where(accommodation => accommodation.Type == type);
        if (availableFrom.HasValue) query = query.Where(accommodation => accommodation.CheckOutDate >= availableFrom);
        if (availableTo.HasValue) query = query.Where(accommodation => accommodation.CheckInDate <= availableTo);

        var accommodations = await query.OrderBy(accommodation => accommodation.CheckInDate).ThenBy(accommodation => accommodation.Name).ToListAsync();
        return Ok(accommodations.Select(accommodation => accommodation.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccommodationDto>> GetById(int id)
    {
        var accommodation = await DetailedQuery().FirstOrDefaultAsync(item =>
            item.Id == id && !item.IsDeleted && !item.Destination.IsDeleted && !item.Destination.Trip.IsDeleted);
        return accommodation == null ? NotFound() : Ok(accommodation.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<AccommodationDto>> Create(AccommodationCreateDto request)
    {
        var destination = await ValidateRequest(request);
        if (destination == null) return ValidationProblem(ModelState);

        var accommodation = new Accommodation
        {
            Name = request.Name.Trim(),
            Type = request.Type!.Value,
            Address = request.Address.Trim(),
            CostPerNight = request.CostPerNight!.Value,
            CheckInDate = request.CheckInDate!.Value,
            CheckOutDate = request.CheckOutDate!.Value,
            DestinationId = request.DestinationId!.Value
        };

        Context.Accommodations.Add(accommodation);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = accommodation.Id }, (await DetailedQuery().FirstAsync(item => item.Id == accommodation.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccommodationDto>> Update(int id, AccommodationUpdateDto request)
    {
        var accommodation = await Context.Accommodations.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (accommodation == null) return NotFound();

        var destination = await ValidateRequest(request);
        if (destination == null) return ValidationProblem(ModelState);

        accommodation.Name = request.Name.Trim();
        accommodation.Type = request.Type!.Value;
        accommodation.Address = request.Address.Trim();
        accommodation.CostPerNight = request.CostPerNight!.Value;
        accommodation.CheckInDate = request.CheckInDate!.Value;
        accommodation.CheckOutDate = request.CheckOutDate!.Value;
        accommodation.DestinationId = request.DestinationId!.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var accommodation = await Context.Accommodations.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (accommodation == null) return NotFound();

        accommodation.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Destination?> ValidateRequest(AccommodationCreateDto request)
    {
        var destination = await GetActiveDestinationWithTrip(request.DestinationId!.Value);
        if (destination == null)
        {
            ModelState.AddModelError(nameof(request.DestinationId), "Active destination does not exist.");
            return null;
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            ModelState.AddModelError(nameof(request.CheckOutDate), "Check-out date must be after check-in date.");
        }

        ValidateDateWithinTrip(request.CheckInDate!.Value, destination, nameof(request.CheckInDate));
        ValidateDateWithinTrip(request.CheckOutDate!.Value, destination, nameof(request.CheckOutDate));
        return ModelState.IsValid ? destination : null;
    }
}
