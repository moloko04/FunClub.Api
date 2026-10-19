using System.ComponentModel.DataAnnotations;

namespace FunClub.Api.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
