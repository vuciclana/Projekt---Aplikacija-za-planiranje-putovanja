using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektLana 
{
    public class Transport 
    {
        public int Id { get; set; }
        public TransportType Type { get; set; }
        public decimal Cost { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
