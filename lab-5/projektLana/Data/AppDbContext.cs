using Microsoft.EntityFrameworkCore;
using projektLana;
using projektLana.Models;

namespace projektLana.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<DestinationPhoto> DestinationPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if needed beyond data annotations
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Destination>()
                .HasOne(d => d.Trip)
                .WithMany(t => t.Destinations)
                .HasForeignKey(d => d.TripId);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Destination)
                .WithMany(d => d.Activities)
                .HasForeignKey(a => a.DestinationId);

            modelBuilder.Entity<Accommodation>()
                .HasOne(a => a.Destination)
                .WithMany(d => d.Accommodations)
                .HasForeignKey(a => a.DestinationId);

            modelBuilder.Entity<Transport>()
                .HasOne(t => t.Destination)
                .WithMany(d => d.Transports)
                .HasForeignKey(t => t.DestinationId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Destination)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DestinationPhoto>()
                .HasOne(p => p.Destination)
                .WithMany(d => d.Photos)
                .HasForeignKey(p => p.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
