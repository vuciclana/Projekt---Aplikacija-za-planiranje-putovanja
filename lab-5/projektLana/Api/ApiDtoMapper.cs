using projektLana.Api.Dtos;

namespace projektLana.Api;

public static class ApiDtoMapper
{
    public static UserSummaryDto ToSummaryDto(this User user) =>
        new(user.Id, $"{user.FirstName} {user.LastName}", user.Email);

    public static UserDto ToDto(this User user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Trips.Count(trip => !trip.IsDeleted),
            user.Reviews.Count(review => !review.IsDeleted));

    public static TripSummaryDto ToSummaryDto(this Trip trip) =>
        new(trip.Id, trip.Name, trip.StartDate, trip.EndDate);

    public static DestinationSummaryDto ToSummaryDto(this Destination destination) =>
        new(destination.Id, destination.City, destination.Country, destination.TripId, destination.Trip.Name);

    public static ActivityDto ToDto(this Activity activity) =>
        new(activity.Id, activity.Name, activity.TypeOfActivity, activity.Date, activity.Cost, activity.Destination.ToSummaryDto());

    public static AccommodationDto ToDto(this Accommodation accommodation) =>
        new(
            accommodation.Id,
            accommodation.Name,
            accommodation.Type,
            accommodation.Address,
            accommodation.CostPerNight,
            accommodation.CheckInDate,
            accommodation.CheckOutDate,
            accommodation.NumberOfNights,
            accommodation.TotalCost,
            accommodation.Destination.ToSummaryDto());

    public static TransportDto ToDto(this Transport transport) =>
        new(
            transport.Id,
            transport.Type,
            transport.Cost,
            transport.DepartureTime,
            transport.ArrivalTime,
            transport.Duration.TotalHours,
            transport.Destination.ToSummaryDto());

    public static ReviewDto ToDto(this Review review) =>
        new(review.Id, review.Rating, review.Comment, review.User.ToSummaryDto(), review.Destination.ToSummaryDto());

    public static DestinationDto ToDto(this Destination destination)
    {
        var activities = destination.Activities
            .Where(item => !item.IsDeleted)
            .Select(item => new DestinationActivityDto(item.Id, item.Name, item.TypeOfActivity, item.Date, item.Cost))
            .ToList();

        var accommodations = destination.Accommodations
            .Where(item => !item.IsDeleted)
            .Select(item => new DestinationAccommodationDto(
                item.Id,
                item.Name,
                item.Type,
                item.Address,
                item.CostPerNight,
                item.CheckInDate,
                item.CheckOutDate,
                item.NumberOfNights,
                item.TotalCost))
            .ToList();

        var transports = destination.Transports
            .Where(item => !item.IsDeleted)
            .Select(item => new DestinationTransportDto(
                item.Id,
                item.Type,
                item.Cost,
                item.DepartureTime,
                item.ArrivalTime,
                item.Duration.TotalHours))
            .ToList();

        var reviews = destination.Reviews
            .Where(item => !item.IsDeleted)
            .Select(item => new DestinationReviewDto(item.Id, item.Rating, item.Comment, item.User.ToSummaryDto()))
            .ToList();
        var totalCost = activities.Sum(item => item.Cost) +
                        accommodations.Sum(item => item.TotalCost) +
                        transports.Sum(item => item.Cost);

        return new DestinationDto(
            destination.Id,
            destination.City,
            destination.Country,
            destination.Description,
            destination.Trip.ToSummaryDto(),
            totalCost,
            activities,
            accommodations,
            transports,
            reviews);
    }

    public static TripDto ToDto(this Trip trip)
    {
        var destinations = trip.Destinations
            .Where(item => !item.IsDeleted)
            .Select(ToTripDestinationDto)
            .ToList();

        return new TripDto(
            trip.Id,
            trip.Name,
            trip.StartDate,
            trip.EndDate,
            trip.User.ToSummaryDto(),
            destinations.Sum(item => item.TotalCost),
            destinations);
    }

    private static TripDestinationDto ToTripDestinationDto(Destination destination)
    {
        var activities = destination.Activities
            .Where(item => !item.IsDeleted)
            .Select(item => new TripActivityDto(item.Id, item.Name, item.TypeOfActivity, item.Date, item.Cost))
            .ToList();

        var accommodations = destination.Accommodations
            .Where(item => !item.IsDeleted)
            .Select(item => new TripAccommodationDto(
                item.Id,
                item.Name,
                item.Type,
                item.Address,
                item.CostPerNight,
                item.CheckInDate,
                item.CheckOutDate,
                item.NumberOfNights,
                item.TotalCost))
            .ToList();

        var transports = destination.Transports
            .Where(item => !item.IsDeleted)
            .Select(item => new TripTransportDto(
                item.Id,
                item.Type,
                item.Cost,
                item.DepartureTime,
                item.ArrivalTime,
                item.Duration.TotalHours))
            .ToList();

        var reviews = destination.Reviews
            .Where(item => !item.IsDeleted)
            .Select(item => new TripReviewDto(item.Id, item.Rating, item.Comment, item.User.ToSummaryDto()))
            .ToList();

        return new TripDestinationDto(
            destination.Id,
            destination.City,
            destination.Country,
            destination.Description,
            activities.Sum(item => item.Cost) +
            accommodations.Sum(item => item.TotalCost) +
            transports.Sum(item => item.Cost),
            activities,
            accommodations,
            transports,
            reviews);
    }
}
