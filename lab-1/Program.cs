
namespace projekt_LV
{
    class Program
    {
        static void Main(string[] args)
        {
            var u1 = new User { Id = 1, FirstName = "Marija", LastName = "Maric", Email = "mmaric@gmail.com" };
            var u2 = new User { Id = 2, FirstName = "Lana", LastName = "Vucic", Email = "lanav@gmail.com" };
            var u3 = new User { Id = 3, FirstName = "Marko", LastName = "Horvat", Email = "horvatko@gmail.com" };

            var reviews = new List<Review>();
            // ------ITALIJA

            var trip1 = new Trip { 
                Id = 1,
                Name = "Italy Trip",
                StartDate = new DateTime(2025, 8, 20),
                EndDate = new DateTime(2025, 8, 28),
                User = u1
            };

            var rome = new Destination
            {
                Id = 1,
                City = "Rome",
                Country = "Italy",
                Description = "Historic city"
            };

            rome.Transports.Add(new Transport
            {
                Id = 1,
                Type = TransportType.Airplane,
                Cost = 150,
                DepartureTime = new DateTime(2025, 8, 20, 8, 0, 0),
                ArrivalTime = new DateTime(2025, 8, 20, 9, 0, 0)
            });

            var milan = new Destination
            {
                Id = 2,
                City = "Milan",
                Country = "Italy",
                Description = "Fashion capital"
            };

            milan.Transports.Add(new Transport
            {
                Id = 2,
                Type = TransportType.Train,
                Cost = 40,
                DepartureTime = new DateTime(2025, 8, 23, 10, 0, 0),
                ArrivalTime = new DateTime(2025, 8, 23, 12, 0, 0)
            });

            var venice = new Destination
            {
                Id = 3,
                City = "Venice",
                Country = "Italy",
                Description = "City of canals"
            };


            venice.Transports.Add(new Transport
            {
                Id = 3,
                Type = TransportType.Airplane,
                Cost = 80,
                DepartureTime = new DateTime(2025, 8, 28, 20, 0, 0),
                ArrivalTime = new DateTime(2025, 8, 28, 21, 30, 0)
            });

            rome.Activities.Add(new Activity { Id = 1, Name = "Colosseum Tour", Cost = 55, Date = new DateTime(2025, 8, 21), TypeOfActivity = ActivityType.Sightseeing });
            milan.Activities.Add(new Activity { Id = 2, Name = "Food Tour", Cost = 70, Date = new DateTime(2025, 8, 22), TypeOfActivity = ActivityType.Food });
            venice.Activities.Add(new Activity { Id = 3, Name = "Museum Visit", Cost = 30, Date = new DateTime(2025, 8, 25), TypeOfActivity = ActivityType.Relaxation });

            reviews.Add(new Review
            {
                Id = 1,
                Rating = 1,
                Comment = "Opljačkali me prilikom posjeta Rimu, pazite se!",
                User = u1,
                Destination = rome
            });

            reviews.Add(new Review
            {
                Id = 2,
                Rating = 4,
                Comment = "Jako lijep grad, jedino što je skup :(",
                User = u1,
                Destination = venice
            });

            rome.Accommodations.Add(new Accommodation { Id = 1, Name = "Hotel Roma", Type = AccommodationType.Hotel, CostPerNight = 120, CheckInDate = new DateTime(2025, 8, 20), CheckOutDate = new DateTime(2025, 8, 28) });

            trip1.Destinations.AddRange(new List<Destination> { rome, milan, venice });

            Console.WriteLine($"Trip ID: {trip1.Id}, Ime putovanja: {trip1.Name}, User: {trip1.User.FirstName} {trip1.User.LastName}, Ukupna cijena: {trip1.TotalCost} eura, Trajanje: {(trip1.EndDate - trip1.StartDate).Days} dana");

            // ------JAPAN

            var trip2 = new Trip
            {
                Id = 2,
                Name = "Japan Trip",
                StartDate = new DateTime(2026, 2, 11),
                EndDate = new DateTime(2026, 2, 25),
                User = u2
            };

            var tokyo = new Destination
            {
                Id = 4,
                City = "Tokyo",
                Country = "Japan",
                Description = "Modern city"
            };

            tokyo.Transports.Add(new Transport
             {
                 Id = 4,
                 Type = TransportType.Airplane,
                 Cost = 550,
                 DepartureTime = new DateTime(2026, 2, 11, 5, 0, 0),
                 ArrivalTime = new DateTime(2026, 2, 11, 16, 0, 0)
             });

            tokyo.Transports.Add(new Transport
            {
                Id = 5,
                Type = TransportType.Train,
                Cost = 120,
                DepartureTime = new DateTime(2026, 2, 19, 9, 0, 0),
                ArrivalTime = new DateTime(2026, 2, 19, 12, 0, 0)
            });

            var kyoto = new Destination
            {
                Id = 5,
                City = "Kyoto",
                Country = "Japan",
                Description = "Temple city"
            };

            kyoto.Transports.Add(new Transport
            {
                Id = 6,
                Type = TransportType.Ferry,
                Cost = 120,
                DepartureTime = new DateTime(2026, 2, 25, 9, 0, 0),
                ArrivalTime = new DateTime(2026, 2, 25, 12, 0, 0)
            });

            tokyo.Activities.Add(new Activity { Id = 4, Name = "Shibuya Crossing", Cost = 0, Date = new DateTime(2026, 2, 13), TypeOfActivity = ActivityType.Sightseeing });
            tokyo.Activities.Add(new Activity { Id = 5, Name = "Sushi Tour", Cost = 100, Date = new DateTime(2026, 2, 14), TypeOfActivity = ActivityType.Food });
            kyoto.Activities.Add(new Activity { Id = 6, Name = "Temple Visit", Cost = 20, Date = new DateTime(2026, 2, 22), TypeOfActivity = ActivityType.Relaxation });

            tokyo.Accommodations.Add(new Accommodation { Id = 2, Name = "Tokyo Capsule Hotel", Type = AccommodationType.Hotel, CostPerNight = 50, CheckInDate = new DateTime(2026, 2, 11), CheckOutDate = new DateTime(2026, 2, 19) });
            kyoto.Accommodations.Add(new Accommodation { Id = 2, Name = "Kyoto Apartment", Type = AccommodationType.Apartment, CostPerNight = 80, CheckInDate = new DateTime(2026, 2, 19), CheckOutDate = new DateTime(2026, 2, 25) });

            reviews.Add(new Review
            {
                Id = 3,
                Rating = 5,
                Comment = "WOW! bez teksta sam",
                User = u2,
                Destination = kyoto
            });

            trip2.Destinations.Add(tokyo);
            trip2.Destinations.Add(kyoto);

            Console.WriteLine($"Trip ID: {trip2.Id}, Ime putovanja: {trip2.Name}, User: {trip2.User.FirstName} {trip2.User.LastName}, Ukupna cijena: {trip2.TotalCost} eura, Trajanje: {(trip2.EndDate - trip2.StartDate).Days} dana");

            // ------ENGLESKA

            var trip3 = new Trip
            {
                Id = 3,
                Name = "England Trip",
                StartDate = new DateTime(2025, 10, 5),
                EndDate = new DateTime(2025, 10, 12),
                User = u3
            };

            var london = new Destination
            {
                Id = 6,
                City = "London",
                Country = "UK",
                Description = "Capital city"
            };

            var manchester = new Destination
            {
                Id = 7,
                City = "Manchester",
                Country = "UK",
                Description = "Football city"
            };

            london.Activities.Add(new Activity { Id = 7, Name = "London Eye", Cost = 35, Date = new DateTime(2025, 10, 6), TypeOfActivity = ActivityType.Sightseeing });
            london.Activities.Add(new Activity { Id = 8, Name = "Museum Visit", Cost = 0, Date = new DateTime(2025, 10, 7), TypeOfActivity = ActivityType.Relaxation });
            london.Activities.Add(new Activity { Id = 9, Name = "Pub Tour", Cost = 60, Date = new DateTime(2025, 10, 8), TypeOfActivity = ActivityType.Party });
            manchester.Activities.Add(new Activity { Id = 10, Name = "Football Stadium Tour", Cost = 50, Date = new DateTime(2025, 10, 10), TypeOfActivity = ActivityType.Sightseeing });

            london.Accommodations.Add(new Accommodation { Id = 3, Name = "London Hotel", Type = AccommodationType.Hotel, CostPerNight = 150, CheckInDate = new DateTime(2025, 10, 5), CheckOutDate = new DateTime(2025, 10, 9) });
            manchester.Accommodations.Add(new Accommodation { Id = 4, Name = "Manchester Apartment", Type = AccommodationType.Apartment, CostPerNight = 100, CheckInDate = new DateTime(2025, 10, 9), CheckOutDate = new DateTime(2025, 10, 12)});

            london.Transports.Add(new Transport
            {
                Id = 7,
                Type = TransportType.Airplane,
                Cost = 220,
                DepartureTime = new DateTime(2025, 10, 5, 12, 0, 0),
                ArrivalTime = new DateTime(2025, 10, 5, 16, 30, 0)
            });

            london.Transports.Add(new Transport
            {
                Id = 8,
                Type = TransportType.Bus,
                Cost = 20,
                DepartureTime = new DateTime(2025, 10, 6, 9, 10, 0),
                ArrivalTime = new DateTime(2025, 10, 6, 10, 0, 0)
            });

            manchester.Transports.Add(new Transport
            {
                Id = 9,
                Type = TransportType.Train,
                Cost = 150,
                DepartureTime = new DateTime(2025, 10, 12, 5, 0, 0),
                ArrivalTime = new DateTime(2025, 10, 12, 13, 50, 0)
            });

            reviews.Add(new Review
            {
                Id = 4,
                Rating = 2,
                Comment = "iskr bzvz...",
                User = u3,
                Destination = manchester
            });

            reviews.Add(new Review
            {
                Id = 4,
                Rating = 4,
                Comment = "Bilo je zanimljivo vidjet Big Ben i London Bridge, ostatak putovanja sam se čuvao da me ne opljačkaju",
                User = u3,
                Destination = london
            });

            trip3.Destinations.Add(london);
            trip3.Destinations.Add(manchester);

            Console.WriteLine($"Trip ID: {trip3.Id}, Ime putovanja: {trip3.Name}, User: {trip3.User.FirstName} {trip3.User.LastName}, Ukupna cijena: {trip3.TotalCost} eura, Trajanje: {(trip3.EndDate - trip3.StartDate).Days} dana");

            //------LINQ 1

            var trips = new List<Trip> { trip1, trip2, trip3 };

            var najskupljePutovanje = trips
                .OrderByDescending(t => t.TotalCost)
                .FirstOrDefault();

            Console.WriteLine($"\nNajskuplje putovanje je: {najskupljePutovanje.Name}, User : {najskupljePutovanje.User.FirstName} {najskupljePutovanje.User.LastName} --> {najskupljePutovanje.TotalCost} eura");


            //------LINQ 2

            Console.WriteLine("\nJako skupe aktivnosti (! 50+ eura :(( !)");

            foreach (var trip in trips)
            {
                var skupeAktivnosti = trip.Destinations
                    .SelectMany(d => d.Activities)
                    .Where(a => a.Cost > 50)
                    .OrderByDescending(a => a.Cost);

                Console.WriteLine($"({trip.Id})Putovanje: {trip.Name}");

                foreach (var aktivnost in skupeAktivnosti)
                {
                    Console.WriteLine($"  - {aktivnost.Name} ({aktivnost.Cost} eura)");
                }

            }


            //------LINQ 3

            var grupirano = trips
                .SelectMany(t => t.Destinations)
                .SelectMany(d => d.Activities)
                .GroupBy(a => a.TypeOfActivity);

            Console.WriteLine("\n-- Prikaz svih aktivnosti sa svih putovanja grupirane prema enumeracijama --");

            foreach (var grupa in grupirano)
            {
                Console.WriteLine($"\nTip: {grupa.Key}");

                foreach (var a in grupa)
                {
                    Console.WriteLine($" - {a.Name}");
                }
            } 


            //------LINQ 4

            var sortedReviews = reviews
                .OrderByDescending(r => r.Rating)
                .ToList();

            Console.WriteLine("\n\n-- Prikaz svih recenzija sortirano prema ocjenama --");

            foreach (var r in sortedReviews)
            {
                Console.WriteLine($"\n{r.User.FirstName} - {r.Destination.City} ( {r.Rating}* )");
                Console.WriteLine($" -> {r.Comment}");
            }
        }
    }
}
