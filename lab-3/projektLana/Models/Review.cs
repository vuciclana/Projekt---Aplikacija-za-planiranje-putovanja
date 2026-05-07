using System;

namespace projektLana
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } 
        public String? Comment { get; set; }
        
        public User? User { get; set; }
        public Destination? Destination { get; set; }
        public string Stars => new string('★', Rating) + new string('☆', 5 - Rating);
        public bool IsPositive => Rating >= 4;
        public string ReviewerName => User != null ? $"{User.FirstName} {User.LastName}" : "Anonymous";
    }
}
