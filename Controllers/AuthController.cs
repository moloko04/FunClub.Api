using FunClub.Api.Data;
using FunClub.Api.DTOs;
using FunClub.Api.Models;
using FunClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunClub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FunClubDbContext _db;
    private readonly IPasswordService _pwd;
    private readonly IConfiguration _config;

    public AuthController(FunClubDbContext db, IPasswordService pwd, IConfiguration config)
    {
        _db = db; _pwd = pwd; _config = config;
    }

    /// <summary>Register a new account (Participant or Lecturer).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Email already registered." });

        var role = "Participant";
        if (string.Equals(dto.Role, "Lecturer", StringComparison.OrdinalIgnoreCase))
        {
            var secret = _config["LecturerSecret"];
            if (string.IsNullOrEmpty(dto.LecturerSecret) || dto.LecturerSecret != secret)
                return BadRequest(new { message = "Invalid lecturer secret." });
            role = "Lecturer";
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            Surname = dto.Surname.Trim(),
            Email = email,
            PasswordHash = _pwd.Hash(dto.Password),
            Role = role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new UserResponseDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            Surname = user.Surname,
            Email = user.Email,
            Role = user.Role
        });
    }

    /// <summary>Login and create an authenticated session.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !_pwd.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        HttpContext.Session.SetUser(user.UserId, user.Role, $"{user.FirstName} {user.Surname}");
        return Ok(new UserResponseDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            Surname = user.Surname,
            Email = user.Email,
            Role = user.Role
        });
    }

    /// <summary>Logout and clear the session.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public IActionResult Logout()
    {
        if (HttpContext.Session.GetUserId() == null) return Unauthorized();
        HttpContext.Session.Clear();
        return NoContent();
    }
}
