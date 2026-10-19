using FunClub.Api.Data;
using FunClub.Api.DTOs;
using FunClub.Api.Models;
using FunClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunClub.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly FunClubDbContext _db;
    public EnrollmentsController(FunClubDbContext db) => _db = db;

    private int? UserId => HttpContext.Session.GetUserId();
    private string? Role => HttpContext.Session.GetRole();

    /// <summary>Enrol the current participant in an event.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
    {
        if (UserId == null) return Unauthorized();
        if (Role != "Participant") return Forbid();

        var ev = await _db.Events.Include(e => e.Enrollments)
            .FirstOrDefaultAsync(e => e.EventId == dto.EventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (await _db.Enrollments.AnyAsync(x => x.ParticipantId == UserId && x.EventId == dto.EventId))
            return Conflict(new { message = "Already enrolled in this event." });

        if (ev.Enrollments.Count >= ev.MaxParticipants)
            return Conflict(new { message = "Event is full." });

        var enr = new Enrollment { ParticipantId = UserId.Value, EventId = dto.EventId };
        _db.Enrollments.Add(enr);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { enrollmentId = enr.EnrollmentId });
    }

    /// <summary>Cancel own enrolment.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id)
    {
        if (UserId == null) return Unauthorized();
        if (Role != "Participant") return Forbid();
        var enr = await _db.Enrollments.FindAsync(id);
        if (enr == null) return NotFound();
        if (enr.ParticipantId != UserId) return Forbid();
        _db.Enrollments.Remove(enr);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get current participant's activities with date, time and venue.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ParticipantSummaryDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> My()
    {
        if (UserId == null) return Unauthorized();
        if (Role != "Participant") return Forbid();

        var user = await _db.Users.FindAsync(UserId.Value);
        var enrollments = await _db.Enrollments
            .Where(e => e.ParticipantId == UserId)
            .Include(e => e.Event)!.ThenInclude(ev => ev!.Category)
            .ToListAsync();

        return Ok(new ParticipantSummaryDto
        {
            Participant = $"{user!.FirstName} {user.Surname}",
            NumberOfActivities = enrollments.Count,
            Activities = enrollments.Select(e => new ActivityDto
            {
                EventName = e.Event!.EventName,
                Category = e.Event.Category!.CategoryName,
                Date = e.Event.EventDate.ToString("yyyy-MM-dd"),
                StartTime = e.Event.StartTime.ToString(@"hh\:mm"),
                Venue = e.Event.Venue
            }).ToList()
        });
    }

    /// <summary>Get the current participant's activity count.</summary>
    [HttpGet("my/count")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> MyCount()
    {
        if (UserId == null) return Unauthorized();
        if (Role != "Participant") return Forbid();
        var count = await _db.Enrollments.CountAsync(e => e.ParticipantId == UserId);
        return Ok(new { numberOfActivities = count });
    }
}