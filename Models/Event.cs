using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FunClub.Api.Models;

public class Event
{
    public int EventId { get; set; }

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

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public int? OrganiserId { get; set; }

    [ForeignKey(nameof(OrganiserId))]
    public User? Organiser { get; set; }

    [Range(1, 1000)]
    public int MaxParticipants { get; set; } = 20;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
