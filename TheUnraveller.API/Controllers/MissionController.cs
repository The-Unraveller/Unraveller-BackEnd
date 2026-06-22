using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System.Security.Claims;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MissionController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MissionDto>>> GetMissions()
    {
        int? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
        {
            userId = id;
        }

        var missions = await _missionService.GetAllMissionsAsync(userId);
        return Ok(missions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MissionDto>> GetMission(int id)
    {
        int? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int uid))
        {
            userId = uid;
        }

        var mission = await _missionService.GetMissionByIdAsync(id, userId);
        if (mission == null) return NotFound();
        return Ok(mission);
    }
}
