using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim.Value);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("update-streak")]
    public async Task<IActionResult> UpdateStreak()
    {
        try
        {
            var userId = GetUserId();
            await _userService.UpdateStreakAsync(userId);
            return Ok(new { Message = "Streak updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("english-level")]
    public async Task<IActionResult> UpdateEnglishLevel([FromBody] UpdateEnglishLevelRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            await _userService.UpdateEnglishLevelAsync(userId, request.EnglishLevel);
            return Ok(new { Message = "Cập nhật trình độ tiếng Anh thành công." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            await _userService.UpdateProfileAsync(userId, request.Username, request.Email);
            return Ok(new { Message = "Cập nhật hồ sơ cá nhân thành công." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }
}
