using FunClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FunClub.Api.Data;

public class FunClubDbContext : DbContext
{
    public FunClubDbContext(DbContextOptions<FunClubDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.ParticipantId, e.EventId })
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Chess", Description = "Strategic board game" },
            new Category { CategoryId = 2, CategoryName = "Football", Description = "Team sport" },
            new Category { CategoryId = 3, CategoryName = "Singing", Description = "Vocal performance" },
            new Category { CategoryId = 4, CategoryName = "Spelling Bee", Description = "Spelling competition" },
            new Category { CategoryId = 5, CategoryName = "Swimming", Description = "Aquatic sport" }
        );
    }
}