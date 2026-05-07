using System;

namespace projektLana 
{
    public class Activity
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public ActivityType TypeOfActivity { get; set; }
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public bool IsFree => Cost == 0;
        public string PriceLabel => IsFree ? "Free" : $"{Cost:0.00} €";
        public bool IsPremium => Cost >= 50;
    }
}
