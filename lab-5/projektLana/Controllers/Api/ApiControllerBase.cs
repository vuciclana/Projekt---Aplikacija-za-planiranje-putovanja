using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;

namespace projektLana.Controllers.Api;

public abstract class ApiControllerBase : ControllerBase
{
    protected readonly AppDbContext Context;

    protected ApiControllerBase(AppDbContext context)
    {
        Context = context;
    }

    protected async Task<Destination?> GetActiveDestinationWithTrip(int destinationId)
    {
        return await Context.Destinations
            .Include(destination => destination.Trip)
            .FirstOrDefaultAsync(destination =>
                destination.Id == destinationId &&
                !destination.IsDeleted &&
                !destination.Trip.IsDeleted);
    }

    protected bool ValidateDateWithinTrip(DateTime value, Destination destination, string fieldName)
    {
        var startDate = destination.Trip.StartDate.Date;
        var endDate = destination.Trip.EndDate.Date.AddDays(1).AddTicks(-1);

        if (value >= startDate && value <= endDate)
        {
            return true;
        }

        ModelState.AddModelError(fieldName, "Date must be within the destination's trip date range.");
        return false;
    }
}
