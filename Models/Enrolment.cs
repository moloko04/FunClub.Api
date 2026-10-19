using System.ComponentModel.DataAnnotations.Schema;

namespace FunClub.Api.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    public int ParticipantId { get; set; }

    [ForeignKey(nameof(ParticipantId))]
    public User? Participant { get; set; }

    public int EventId { get; set; }

    [ForeignKey(nameof(EventId))]
    public Event? Event { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}