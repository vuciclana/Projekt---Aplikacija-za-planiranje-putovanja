using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektLana
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public String Comment { get; set; }
        public User User { get; set; }
        public Destination Destination { get; set; }
          
    }
}
