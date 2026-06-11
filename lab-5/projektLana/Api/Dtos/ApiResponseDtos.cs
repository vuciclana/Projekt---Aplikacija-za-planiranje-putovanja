namespace projektLana.Api.Dtos;

public record UserSummaryDto(int Id, string FullName, string Email);

public record UserDto(int Id, string FirstName, string LastName, string Email, int TripCount, int ReviewCount);

public record TripSummaryDto(int Id, string Name, DateTime StartDate, DateTime EndDate);

public record DestinationSummaryDto(int Id, string City, string Country, int TripId, string TripName);

public record ActivityDto(
    int Id,
    string Name,
    ActivityType Type,
    DateTime Date,
    decimal Cost,
    DestinationSummaryDto Destination);

public record AccommodationDto(
    int Id,
    string Name,
    AccommodationType Type,
    string Address,
    decimal CostPerNight,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfNights,
    decimal TotalCost,
    DestinationSummaryDto Destination);

public record TransportDto(
    int Id,
    TransportType Type,
    decimal Cost,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    double DurationHours,
    DestinationSummaryDto Destination);

public record ReviewDto(
    int Id,
    int Rating,
    string? Comment,
    UserSummaryDto Reviewer,
    DestinationSummaryDto Destination);

public record DestinationActivityDto(int Id, string Name, ActivityType Type, DateTime Date, decimal Cost);

public record DestinationAccommodationDto(
    int Id,
    string Name,
    AccommodationType Type,
    string Address,
    decimal CostPerNight,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfNights,
    decimal TotalCost);

public record DestinationTransportDto(
    int Id,
    TransportType Type,
    decimal Cost,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    double DurationHours);

public record DestinationReviewDto(int Id, int Rating, string? Comment, UserSummaryDto Reviewer);

public record DestinationDto(
    int Id,
    string City,
    string Country,
    string Description,
    TripSummaryDto Trip,
    decimal TotalCost,
    IReadOnlyList<DestinationActivityDto> Activities,
    IReadOnlyList<DestinationAccommodationDto> Accommodations,
    IReadOnlyList<DestinationTransportDto> Transports,
    IReadOnlyList<DestinationReviewDto> Reviews);

public record TripActivityDto(int Id, string Name, ActivityType Type, DateTime Date, decimal Cost);

public record TripAccommodationDto(
    int Id,
    string Name,
    AccommodationType Type,
    string Address,
    decimal CostPerNight,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfNights,
    decimal TotalCost);

public record TripTransportDto(
    int Id,
    TransportType Type,
    decimal Cost,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    double DurationHours);

public record TripReviewDto(int Id, int Rating, string? Comment, UserSummaryDto Reviewer);

public record TripDestinationDto(
    int Id,
    string City,
    string Country,
    string Description,
    decimal TotalCost,
    IReadOnlyList<TripActivityDto> Activities,
    IReadOnlyList<TripAccommodationDto> Accommodations,
    IReadOnlyList<TripTransportDto> Transports,
    IReadOnlyList<TripReviewDto> Reviews);

public record TripDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    UserSummaryDto Traveler,
    decimal TotalCost,
    IReadOnlyList<TripDestinationDto> Destinations);
