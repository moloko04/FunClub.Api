using FunClub.Api.Data;
using FunClub.Api.DTOs;
using FunClub.Api.Models;
using FunClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunClub.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly FunClubDbContext _db;
    public EventsController(FunClubDbContext db) => _db = db;

    private bool IsAuthenticated => HttpContext.Session.GetUserId() != null;
    private bool IsLecturer => HttpContext.Session.GetRole() == "Lecturer";

    /// <summary>List all events.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAuthenticated) return Unauthorized();
        var list = await _db.Events.Include(e => e.Category).Include(e => e.Enrollments)
            .Select(e => new EventResponseDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                EventDate = e.EventDate,
                StartTime = e.StartTime,
                Venue = e.Venue,
                CategoryId = e.CategoryId,
                CategoryName = e.Category!.CategoryName,
                MaxParticipants = e.MaxParticipants,
                CurrentEnrollments = e.Enrollments.Count
            }).ToListAsync();
        return Ok(list);
    }

    /// <summary>Get an event by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(int id)
    {
        if (!IsAuthenticated) return Unauthorized();
        var e = await _db.Events.Include(x => x.Category).Include(x => x.Enrollments)
            .FirstOrDefaultAsync(x => x.EventId == id);
        return e == null ? NotFound() : Ok(Map(e));
    }

    /// <summary>Create an event (Lecturer only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest(new { message = "Invalid CategoryId." });

        var e = new Event
        {
            EventName = dto.EventName.Trim(),
            EventDate = dto.EventDate,
            StartTime = dto.StartTime,
            Venue = dto.Venue.Trim(),
            CategoryId = dto.CategoryId,
            MaxParticipants = dto.MaxParticipants,
            OrganiserId = HttpContext.Session.GetUserId()
        };
        _db.Events.Add(e);
        await _db.SaveChangesAsync();
        await _db.Entry(e).Reference(x => x.Category).LoadAsync();
        return CreatedAtAction(nameof(Get), new { id = e.EventId }, Map(e));
    }

    /// <summary>Update an event (Lecturer only).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound();
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest(new { message = "Invalid CategoryId." });

        e.EventName = dto.EventName.Trim(); e.EventDate = dto.EventDate;
        e.StartTime = dto.StartTime; e.Venue = dto.Venue.Trim();
        e.CategoryId = dto.CategoryId; e.MaxParticipants = dto.MaxParticipants;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Delete an event (Lecturer only).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound();
        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static EventResponseDto Map(Event e) => new()
    {
        EventId = e.EventId,
        EventName = e.EventName,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Venue = e.Venue,
        CategoryId = e.CategoryId,
        CategoryName = e.Category?.CategoryName ?? string.Empty,
        MaxParticipants = e.MaxParticipants,
        CurrentEnrollments = e.Enrollments?.Count ?? 0
    };
}