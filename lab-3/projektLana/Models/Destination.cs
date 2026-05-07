using System;
using System.Collections.Generic;
using System.Linq;

namespace projektLana
{
    public class Destination  
    {
        public int Id { get; set; }
        public String? City { get; set; }
        public String? Country { get; set; }
        public String? Description { get; set; }

        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public List<Transport> Transports { get; set; } = new List<Transport>();
        public decimal TotalDestinationCost => 
            Activities.Sum(a => a.Cost) + 
            Transports.Sum(t => t.Cost) + 
            Accommodations.Sum(a => a.TotalCost);
            
        public int ActivityCount => Activities.Count;
        public bool HasAccommodation => Accommodations.Any();
        public string Location => $"{City}, {Country}";
    }
}
