using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("skill-map")]
    public async Task<ActionResult<SkillMapDto>> GetSkillMap()
    {
        var userId = GetUserIdFromClaims();
        var result = await _progressService.GetSkillMapAsync(userId);
        return Ok(result);
    }

    [HttpGet("portfolio")]
    public async Task<ActionResult<List<PortfolioEntryDto>>> GetPortfolio()
    {
        var userId = GetUserIdFromClaims();
        var result = await _progressService.GetPortfolioAsync(userId);
        return Ok(result);
    }

    [HttpGet("weekly-report")]
    public async Task<ActionResult<WeeklyReportDto>> GetWeeklyReport()
    {
        var userId = GetUserIdFromClaims();
        var result = await _progressService.GetWeeklyReportAsync(userId);
        return Ok(result);
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim.Value);
    }
}
