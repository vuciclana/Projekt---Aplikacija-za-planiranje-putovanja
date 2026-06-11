using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Api;
using projektLana.Api.Dtos;
using projektLana.Data;

namespace projektLana.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : ApiControllerBase
{
    public UsersApiController(AppDbContext context) : base(context) { }

    private IQueryable<User> DetailedQuery() =>
        Context.Users
            .AsNoTracking()
            .Include(user => user.Trips)
            .Include(user => user.Reviews);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] string? q)
    {
        var query = DetailedQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(user =>
                user.FirstName.ToLower().Contains(search) ||
                user.LastName.ToLower().Contains(search) ||
                user.Email.ToLower().Contains(search));
        }

        var users = await query.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToListAsync();
        return Ok(users.Select(user => user.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await DetailedQuery().FirstOrDefaultAsync(item => item.Id == id);
        return user == null ? NotFound() : Ok(user.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(UserCreateDto request)
    {
        if (await Context.Users.AnyAsync(user => user.Email == request.Email.Trim()))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is already in use.");
            return ValidationProblem(ModelState);
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim()
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, UserUpdateDto request)
    {
        var user = await Context.Users.FirstOrDefaultAsync(item => item.Id == id);
        if (user == null) return NotFound();

        if (await Context.Users.AnyAsync(item => item.Id != id && item.Email == request.Email.Trim()))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is already in use.");
            return ValidationProblem(ModelState);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();
        await Context.SaveChangesAsync();

        return Ok((await DetailedQuery().FirstAsync(item => item.Id == id)).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await Context.Users
            .Include(item => item.Trips)
            .Include(item => item.Reviews)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null) return NotFound();

        if (user.Trips.Count != 0 || user.Reviews.Count != 0)
        {
            return Conflict(new ProblemDetails
            {
                Title = "User cannot be deleted.",
                Detail = "Reassign the user's trips and reviews first.",
                Status = StatusCodes.Status409Conflict
            });
        }

        Context.Users.Remove(user);
        await Context.SaveChangesAsync();
        return NoContent();
    }
}
