using System.ComponentModel.DataAnnotations;

namespace FunClub.Api.DTOs;

public class EnrollmentCreateDto
{
    [Required]
    public int EventId { get; set; }
}

public class ActivityDto
{
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
}

public class ParticipantSummaryDto
{
    public string Participant { get; set; } = string.Empty;
    public int NumberOfActivities { get; set; }
    public List<ActivityDto> Activities { get; set; } = new();
}