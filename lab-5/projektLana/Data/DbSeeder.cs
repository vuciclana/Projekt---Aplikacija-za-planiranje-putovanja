using System.Linq;

namespace projektLana.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Users.Any()) return;

            // 1) add users and save so they get DB ids
            var userMap = MockRepository.Users
                .Select(u => new User { FirstName = u.FirstName, LastName = u.LastName, Email = u.Email })
                .ToList();
            context.Users.AddRange(userMap);
            context.SaveChanges();

            // helper to find saved user by email
            User FindSavedUser(User src) => context.Users.Single(u => u.Email == src.Email);

            // 2) add trips + nested destinations/activities/accommodations/transports/reviews
            var trips = new List<Trip>();
            foreach (var t in MockRepository.Trips)
            {
                var newTrip = new Trip
                {
                    Name = t.Name,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    User = FindSavedUser(t.User)
                };

                foreach (var d in t.Destinations)
                {
                    var newDest = new Destination
                    {
                        City = d.City,
                        Country = d.Country,
                        Description = d.Description
                    };

                    newDest.Activities = d.Activities.Select(a => new Activity {
                        Name = a.Name, TypeOfActivity = a.TypeOfActivity, Date = a.Date, Cost = a.Cost
                    }).ToList();

                    newDest.Accommodations = d.Accommodations.Select(ac => new Accommodation {
                        Name = ac.Name, Type = ac.Type, Address = ac.Address, CostPerNight = ac.CostPerNight,
                        CheckInDate = ac.CheckInDate, CheckOutDate = ac.CheckOutDate
                    }).ToList();

                    newDest.Transports = d.Transports.Select(tr => new Transport {
                        Type = tr.Type, Cost = tr.Cost, DepartureTime = tr.DepartureTime, ArrivalTime = tr.ArrivalTime
                    }).ToList();

                    newTrip.Destinations.Add(newDest);
                }

                trips.Add(newTrip);
            }

            context.Trips.AddRange(trips);
            context.SaveChanges();

            // 3) add reviews mapping to saved users/destinations
            var reviews = MockRepository.Reviews.Select(r => new Review {
                Rating = r.Rating,
                Comment = r.Comment,
                User = context.Users.Single(u => u.Email == r.User.Email),
                Destination = context.Destinations.Single(d => d.City == r.Destination.City && d.Country == r.Destination.Country)
            }).ToList();

            context.Reviews.AddRange(reviews);
            context.SaveChanges();
        }
    }
}