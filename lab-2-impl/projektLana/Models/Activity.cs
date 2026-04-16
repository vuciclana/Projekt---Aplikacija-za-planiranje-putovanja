using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektLana 
{
    public class Activity
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public ActivityType TypeOfActivity { get; set; }
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }

    }
}
