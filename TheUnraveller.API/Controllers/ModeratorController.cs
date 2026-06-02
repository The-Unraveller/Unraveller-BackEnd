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

    [HttpGet("npcs")]
    public async Task<IActionResult> GetNpcs()
    {
        var npcs = await _missionManagementService.GetAllNpcsAsync();
        return Ok(npcs);
    }

    [HttpPost("npcs")]
    public async Task<IActionResult> CreateNpc([FromBody] NpcCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        try
        {
            var npc = await _missionManagementService.CreateNpcAsync(dto);
            return Ok(npc);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("npcs/{id}")]
    public async Task<IActionResult> UpdateNpc(int id, [FromBody] NpcCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        try
        {
            await _missionManagementService.UpdateNpcAsync(id, dto);
            return Ok(new { message = "NPC updated successfully" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
