namespace projektLana.Tests;

internal static class IntegrationTestData
{
    public static Trip CreateTrip(string name = "Test Trip") =>
        new()
        {
            Name = name,
            StartDate = new DateTime(2027, 6, 1),
            EndDate = new DateTime(2027, 6, 15)
        };

    public static Destination AddDestination(Trip trip, string city = "Zagreb")
    {
        var destination = new Destination
        {
            City = city,
            Country = "Croatia",
            Description = $"Visit {city}"
        };
        trip.Destinations.Add(destination);
        return destination;
    }
}
