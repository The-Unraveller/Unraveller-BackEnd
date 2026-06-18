using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BadgeController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgeController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    [HttpGet("my-badges")]
    public async Task<ActionResult<List<UserBadgeDto>>> GetMyBadges()
    {
        var userId = GetUserIdFromClaims();
        var badges = await _badgeService.GetUserBadgesAsync(userId);
        return Ok(badges);
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim.Value);
    }
}
