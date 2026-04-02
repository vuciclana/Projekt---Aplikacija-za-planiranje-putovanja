using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projekt_LV
{ 
    public class Trip
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Destination> Destinations { get; set; } = new List<Destination>();
        public User User { get; set; }

        public decimal TotalCost
        {
            get
            {
                decimal total = 0;

                foreach (var destination in Destinations)
                {
                    foreach (var activity in destination.Activities)
                    {
                        total += activity.Cost;
                    }

                    foreach (var accommodation in destination.Accommodations)
                    {
                        int nights = (accommodation.CheckOutDate - accommodation.CheckInDate).Days;
                        total += accommodation.CostPerNight * nights;
                    }

                    foreach (var transport in destination.Transports)
                    {
                        total += transport.Cost;
                    }
                }

                return total;
            }
        }
    }
}
