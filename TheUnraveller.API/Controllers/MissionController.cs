using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

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
        var missions = await _missionService.GetAllMissionsAsync();
        return Ok(missions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MissionDto>> GetMission(int id)
    {
        var mission = await _missionService.GetMissionByIdAsync(id);
        if (mission == null) return NotFound();
        return Ok(mission);
    }
}
