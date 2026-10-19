using FunClub.Api.Data;
using FunClub.Api.DTOs;
using FunClub.Api.Models;
using FunClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunClub.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly FunClubDbContext _db;
    public CategoriesController(FunClubDbContext db) => _db = db;

    private bool IsAuthenticated => HttpContext.Session.GetUserId() != null;
    private bool IsLecturer => HttpContext.Session.GetRole() == "Lecturer";

    /// <summary>List all categories (authenticated users).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAuthenticated) return Unauthorized();
        var list = await _db.Categories.Select(c => new CategoryResponseDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Description = c.Description
        }).ToListAsync();
        return Ok(list);
    }

    /// <summary>Get a category by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(int id)
    {
        if (!IsAuthenticated) return Unauthorized();
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return NotFound();
        return Ok(new CategoryResponseDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Description = c.Description
        });
    }

    /// <summary>Create a category (Lecturer only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var c = new Category { CategoryName = dto.CategoryName.Trim(), Description = dto.Description };
        _db.Categories.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.CategoryId }, new CategoryResponseDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Description = c.Description
        });
    }

    /// <summary>Update a category (Lecturer only).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateDto dto)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return NotFound();
        c.CategoryName = dto.CategoryName.Trim();
        c.Description = dto.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Delete a category (Lecturer only).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAuthenticated) return Unauthorized();
        if (!IsLecturer) return Forbid();
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return NotFound();
        if (await _db.Events.AnyAsync(e => e.CategoryId == id))
            return Conflict(new { message = "Category has events and cannot be deleted." });
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}