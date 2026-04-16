using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektLana
{
    public class Accommodation
    {
        public int Id { get; set; }
        public String Name { get; set; } 
        public AccommodationType Type { get; set; }
        public String Address { get; set; }
        public decimal CostPerNight { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

    }
}
