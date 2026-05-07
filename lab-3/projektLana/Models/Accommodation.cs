using System;

namespace projektLana
{
    public class Accommodation
    {
        public int Id { get; set; }
        public String? Name { get; set; } 
        public AccommodationType Type { get; set; }
        public String? Address { get; set; }
        public decimal CostPerNight { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights => (CheckOutDate - CheckInDate).Days > 0 ? (CheckOutDate - CheckInDate).Days : 1;
        public decimal TotalCost => CostPerNight * NumberOfNights;
        public string StayPeriod => $"{CheckInDate:dd.MM.yyyy} - {CheckOutDate:dd.MM.yyyy}";
    }
}
