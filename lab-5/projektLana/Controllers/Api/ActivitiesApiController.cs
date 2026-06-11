using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/activities")]
public class ActivitiesApiController : ApiControllerBase
{
    public ActivitiesApiController(AppDbContext context) : base(context) { }

    private IQueryable<Activity> DetailedQuery() =>
        Context.Activities
            .AsNoTracking()
            .Include(activity => activity.Destination)
                .ThenInclude(destination => destination.Trip);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? destinationId,
        [FromQuery] ActivityType? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = DetailedQuery().Where(activity =>
            !activity.IsDeleted &&
            !activity.Destination.IsDeleted &&
            !activity.Destination.Trip.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(activity =>
                activity.Name.ToLower().Contains(search) ||
                activity.Destination.City.ToLower().Contains(search) ||
                activity.Destination.Country.ToLower().Contains(search));
        }

        if (destinationId.HasValue) query = query.Where(activity => activity.DestinationId == destinationId);
        if (type.HasValue) query = query.Where(activity => activity.TypeOfActivity == type);
        if (from.HasValue) query = query.Where(activity => activity.Date >= from);
        if (to.HasValue) query = query.Where(activity => activity.Date <= to);

        var activities = await query.OrderBy(activity => activity.Date).ThenBy(activity => activity.Name).ToListAsync();
        return Ok(activities.Select(activity => activity.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id)
    {
        var activity = await DetailedQuery().FirstOrDefaultAsync(item =>
            item.Id == id && !item.IsDeleted && !item.Destination.IsDeleted && !item.Destination.Trip.IsDeleted);
        return activity == null ? NotFound() : Ok(activity.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(ActivityCreateDto request)
    {
        var destination = await GetActiveDestinationWithTrip(request.DestinationId!.Value);
        if (destination == null)
        {
            ModelState.AddModelError(nameof(request.DestinationId), "Active destination does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!ValidateDateWithinTrip(request.Date!.Value, destination, nameof(request.Date)))
        {
            return ValidationProblem(ModelState);
        }

        var activity = new Activity
        {
            Name = request.Name.Trim(),
            TypeOfActivity = request.Type!.Value,
            Date = request.Date.Value,
            Cost = request.Cost!.Value,
            DestinationId = request.DestinationId.Value
        };

        Context.Activities.Add(activity);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, (await DetailedQuery().FirstAsync(item => item.Id == activity.Id)).ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ActivityDto>> Update(int id, ActivityUpdateDto request)
    {
        var activity = await Context.Activities.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (activity == null) return NotFound();

        var destination = await GetActiveDestinationWithTrip(request.DestinationId!.Value);
        if (destination == null)
        {
            ModelState.AddModelError(nameof(request.DestinationId), "Active destination does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!ValidateDateWithinTrip(request.Date!.Value, destination, nameof(request.Date)))
        {
            return ValidationProblem(ModelState);
        }

        activity.Name = request.Name.Trim();
        activity.TypeOfActivity = request.Type!.Value;
        activity.Date = request.Date.Value;
        activity.Cost = request.Cost!.Value;
        activity.DestinationId = request.DestinationId.Value;
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var activity = await Context.Activities.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (activity == null) return NotFound();

        activity.IsDeleted = true;
        await Context.SaveChangesAsync();
        return NoContent();
    }
}
