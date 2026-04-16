using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektLana
{
    public class Destination  
    {
        public int Id { get; set; }
        public String City { get; set; }
        public String Country { get; set; }
        public String Description { get; set; }

        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public List<Transport> Transports { get; set; } = new List<Transport>();
    }
}
