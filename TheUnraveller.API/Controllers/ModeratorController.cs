using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModeratorController : ControllerBase
{
    private readonly IMissionManagementService _missionManagementService;

    public ModeratorController(IMissionManagementService missionManagementService)
    {
        _missionManagementService = missionManagementService;
    }

    [HttpGet("missions")]
    public async Task<IActionResult> GetMissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var missions = await _missionManagementService.GetAllMissionsForManagementAsync();
        return Ok(missions);
    }

    [HttpPost("missions")]
    public async Task<IActionResult> CreateMission([FromBody] MissionCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        
        int creatorId = int.Parse(userIdClaim.Value);

        try
        {
            await _missionManagementService.CreateMissionAsync(dto, creatorId);
            return Ok(new { message = "Mission submitted for approval successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
