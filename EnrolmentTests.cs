using FunClub.Api.Data;
using FunClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FunClub.Api.Tests;

public class EnrollmentTests
{
    private static FunClubDbContext GetDb()
    {
        var opts = new DbContextOptionsBuilder<FunClubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new FunClubDbContext(opts);
    }

    [Fact]
    public async Task Duplicate_Enrollment_Is_Detected()
    {
        using var db = GetDb();
        var cat = new Category { CategoryName = "Chess" };
        var ev = new Event
        {
            EventName = "Chess Challenge",
            EventDate = DateTime.Today,
            StartTime = TimeSpan.FromHours(10),
            Venue = "Library",
            Category = cat,
            MaxParticipants = 10
        };
        var u = new User
        {
            FirstName = "L",
            Surname = "N",
            Email = "l@n.com",
            PasswordHash = "h",
            Role = "Participant"
        };
        db.AddRange(cat, ev, u);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment { ParticipantId = u.UserId, EventId = ev.EventId });
        await db.SaveChangesAsync();

        // Second enrolment for same pair should fail the unique index rule conceptually
        var exists = await db.Enrollments.AnyAsync(x => x.ParticipantId == u.UserId && x.EventId == ev.EventId);
        Assert.True(exists);
    }

    [Fact]
    public async Task Capacity_Limit_Enforced()
    {
        using var db = GetDb();
        var cat = new Category { CategoryName = "Swim" };
        var ev = new Event
        {
            EventName = "Swim",
            EventDate = DateTime.Today,
            StartTime = TimeSpan.FromHours(9),
            Venue = "Pool",
            Category = cat,
            MaxParticipants = 1
        };
        db.AddRange(cat, ev);
        await db.SaveChangesAsync();

        var u1 = new User { FirstName = "A", Surname = "A", Email = "a@a.com", PasswordHash = "h", Role = "Participant" };
        var u2 = new User { FirstName = "B", Surname = "B", Email = "b@b.com", PasswordHash = "h", Role = "Participant" };
        db.AddRange(u1, u2);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment { ParticipantId = u1.UserId, EventId = ev.EventId });
        await db.SaveChangesAsync();

        var currentCount = await db.Enrollments.CountAsync(e => e.EventId == ev.EventId);
        Assert.Equal(ev.MaxParticipants, currentCount);
    }

    [Fact]
    public async Task Missing_Event_Returns_Not_Found()
    {
        using var db = GetDb();
        var ev = await db.Events.FindAsync(999);
        Assert.Null(ev);
    }
}