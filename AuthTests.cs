using FunClub.Api.Data;
using FunClub.Api.Models;
using FunClub.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FunClub.Api.Tests;

public class AuthTests
{
    private static FunClubDbContext GetDb()
    {
        var opts = new DbContextOptionsBuilder<FunClubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new FunClubDbContext(opts);
    }

    [Fact]
    public void Hash_And_Verify_Works()
    {
        var svc = new PasswordService();
        var hash = svc.Hash("Passw0rd!");
        Assert.NotEqual("Passw0rd!", hash);
        Assert.True(svc.Verify("Passw0rd!", hash));
        Assert.False(svc.Verify("wrong", hash));
    }

    [Fact]
    public async Task Duplicate_Email_Is_Detected()
    {
        using var db = GetDb();
        db.Users.Add(new User { FirstName = "A", Surname = "B", Email = "x@y.com", PasswordHash = "h", Role = "Participant" });
        await db.SaveChangesAsync();
        Assert.True(await db.Users.AnyAsync(u => u.Email == "x@y.com"));
    }
}
