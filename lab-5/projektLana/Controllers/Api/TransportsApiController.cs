using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/transports")]
public class TransportsApiController : ApiControllerBase
{
    public TransportsApiController(AppDbContext context) : base(context) { }

    private IQueryable<Transport> DetailedQuery() =>
        Context.Transports
            .AsNoTracking()
            .Include(transport => transport.Destination)
                .ThenInclude(destination => destination.Trip);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransportDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? destinationId,
        [FromQuery] TransportType? type,
        [FromQuery] DateTime? departsFrom,
        [FromQuery] DateTime? arrivesBy)
    {
        var query = DetailedQuery().Where(transport =>
            !transport.IsDeleted &&
            !transport.Destination.IsDeleted &&
            !transport.Destination.Trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(transport =>
                transport.Destination.City.ToLower().Contains(search) ||
                transport.Destination.Country.ToLower().Contains(search));
        }

        if (destinationId.HasValue) query = query.Where(transport => transport.DestinationId == destinationId);
        if (type.HasValue) query = query.Where(transport => transport.Type == type);
        if (departsFrom.HasValue) query = query.Where(transport => transport.DepartureTime >= departsFrom);
        if (arrivesBy.HasValue) query = query.Where(transport => transport.ArrivalTime <= arrivesBy);

        var transports = await query.OrderBy(transport => transport.DepartureTime).ToListAsync();
        return Ok(transports.Select(transport => transport.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransportDto>> GetById(int id)
    {
        var transport = await DetailedQuery().FirstOrDefaultAsync(item =>
            item.Id == id && !item.IsDeleted && !item.Destination.IsDeleted && !item.Destination.Trip.IsDeleted);
        return transport == null ? NotFound() : Ok(transport.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TransportDto>> Create(TransportCreateDto request)
    {
        var destination = await ValidateRequest(request);
        if (destination == null) return ValidationProblem(ModelState);

        var transport = new Transport
        {
            Type = request.Type!.Value,
            Cost = request.Cost!.Value,
            DepartureTime = request.DepartureTime!.Value,
            ArrivalTime = request.ArrivalTime!.Value,
            DestinationId = request.DestinationId!.Value
        };

        Context.Transports.Add(transport);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = transport.Id }, (await DetailedQuery().FirstAsync(item => item.Id == transport.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransportDto>> Update(int id, TransportUpdateDto request)
    {
        var transport = await Context.Transports.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (transport == null) return NotFound();

        var destination = await ValidateRequest(request);
        if (destination == null) return ValidationProblem(ModelState);

        transport.Type = request.Type!.Value;
        transport.Cost = request.Cost!.Value;
        transport.DepartureTime = request.DepartureTime!.Value;
        transport.ArrivalTime = request.ArrivalTime!.Value;
        transport.DestinationId = request.DestinationId!.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transport = await Context.Transports.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (transport == null) return NotFound();

        transport.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Destination?> ValidateRequest(TransportCreateDto request)
    {
        var destination = await GetActiveDestinationWithTrip(request.DestinationId!.Value);
        if (destination == null)
        {
            ModelState.AddModelError(nameof(request.DestinationId), "Active destination does not exist.");
            return null;
        }

        if (request.ArrivalTime <= request.DepartureTime)
        {
            ModelState.AddModelError(nameof(request.ArrivalTime), "Arrival time must be after departure time.");
        }

        ValidateDateWithinTrip(request.DepartureTime!.Value, destination, nameof(request.DepartureTime));
        ValidateDateWithinTrip(request.ArrivalTime!.Value, destination, nameof(request.ArrivalTime));
        return ModelState.IsValid ? destination : null;
    }
}
