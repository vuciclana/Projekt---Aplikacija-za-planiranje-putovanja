using System.ComponentModel.DataAnnotations;

namespace projektLana.Api.Dtos;

public class UserCreateDto
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class UserUpdateDto : UserCreateDto;

public class TripCreateDto
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime? StartDate { get; set; }

    [Required]
    public DateTime? EndDate { get; set; }

    [Required]
    public int? UserId { get; set; }
}

public class TripUpdateDto : TripCreateDto;

public class DestinationCreateDto
{
    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int? TripId { get; set; }
}

public class DestinationUpdateDto : DestinationCreateDto;

public class ActivityCreateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EnumDataType(typeof(ActivityType))]
    public ActivityType? Type { get; set; }

    [Required]
    public DateTime? Date { get; set; }

    [Required, Range(typeof(decimal), "0", "100000")]
    public decimal? Cost { get; set; }

    [Required]
    public int? DestinationId { get; set; }
}

public class ActivityUpdateDto : ActivityCreateDto;

public class AccommodationCreateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EnumDataType(typeof(AccommodationType))]
    public AccommodationType? Type { get; set; }

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, Range(typeof(decimal), "0", "100000")]
    public decimal? CostPerNight { get; set; }

    [Required]
    public DateTime? CheckInDate { get; set; }

    [Required]
    public DateTime? CheckOutDate { get; set; }

    [Required]
    public int? DestinationId { get; set; }
}

public class AccommodationUpdateDto : AccommodationCreateDto;

public class TransportCreateDto
{
    [Required, EnumDataType(typeof(TransportType))]
    public TransportType? Type { get; set; }

    [Required, Range(typeof(decimal), "0", "100000")]
    public decimal? Cost { get; set; }

    [Required]
    public DateTime? DepartureTime { get; set; }

    [Required]
    public DateTime? ArrivalTime { get; set; }

    [Required]
    public int? DestinationId { get; set; }
}

public class TransportUpdateDto : TransportCreateDto;

public class ReviewCreateDto
{
    [Required, Range(1, 5)]
    public int? Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    [Required]
    public int? UserId { get; set; }

    [Required]
    public int? DestinationId { get; set; }
}

public class ReviewUpdateDto : ReviewCreateDto;
