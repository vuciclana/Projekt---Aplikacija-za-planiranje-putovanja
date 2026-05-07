using System;

namespace projektLana 
{
    public class Transport 
    {
        public int Id { get; set; }
        public TransportType Type { get; set; }
        public decimal Cost { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TimeSpan Duration => ArrivalTime - DepartureTime;
        public string RouteTime => $"{DepartureTime:dd.MM.yyyy HH:mm} - {ArrivalTime:HH:mm}";
        public bool IsLongTrip => Duration.TotalHours > 5;
    }
}
