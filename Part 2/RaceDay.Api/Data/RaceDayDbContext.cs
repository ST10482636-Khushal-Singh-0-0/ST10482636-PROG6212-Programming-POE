using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Email unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Enrolment unique (Participant can only enter a category once)
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.CategoryId, e.ParticipantId })
                .IsUnique();

            // Unique race number per category
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.CategoryId, e.RaceNumber })
                .IsUnique();

            // 1-to-1 Enrolment to Result
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();
        }
    }
}