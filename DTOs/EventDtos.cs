using System.ComponentModel.DataAnnotations;

namespace FunClub.Api.DTOs;

public class EventCreateDto
{
    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required, MaxLength(100)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Range(1, 1000)]
    public int MaxParticipants { get; set; } = 20;
}

public class EventUpdateDto : EventCreateDto { }

public class EventResponseDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int CurrentEnrollments { get; set; }
}