using System.ComponentModel.DataAnnotations;

namespace FunClub.Api.DTOs;

public class CategoryCreateDto
{
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}

public class CategoryResponseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}