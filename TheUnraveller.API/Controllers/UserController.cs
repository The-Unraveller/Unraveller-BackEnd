using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;

    public UserController(IUserService userService, AppDbContext context)
    {
        _userService = userService;
        _context = context;
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

    public class UserFeedbackSubmitDto
    {
        public int Rating { get; set; } = 5;
        public string Category { get; set; } = "Trải nghiệm UI/UX";
        public string Comment { get; set; } = string.Empty;
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> SubmitFeedback([FromBody] UserFeedbackSubmitDto request)
    {
        try
        {
            var userId = GetUserId();
            var feedback = new Feedback
            {
                UserId = userId,
                Rating = Math.Clamp(request.Rating, 1, 5),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "Trải nghiệm UI/UX" : request.Category,
                Comment = request.Comment ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cảm ơn bạn đã gửi đóng góp phản hồi! Ý kiến của bạn đã được chuyển đến ban quản trị." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
