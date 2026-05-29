using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMissionManagementService _missionManagementService;

    public AdminController(IUserRepository userRepository, IMissionManagementService missionManagementService)
    {
        _userRepository = userRepository;
        _missionManagementService = missionManagementService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users.Select(u => new {
            u.Id, u.Username, u.Email, u.Role, u.XpBalance, u.Energy, u.IsPremium
        }));
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto update)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        if (update.XpBalance.HasValue) user.XpBalance = update.XpBalance.Value;
        if (update.Energy.HasValue) user.Energy = update.Energy.Value;
        if (update.Role.HasValue) user.Role = (UserRole)update.Role.Value;
        if (update.IsPremium.HasValue) user.IsPremium = update.IsPremium.Value;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return Ok(new { message = "User updated successfully" });
    }

    [HttpGet("missions")]
    public async Task<IActionResult> GetAllMissions()
    {
        var missions = await _missionManagementService.GetAllMissionsForManagementAsync();
        return Ok(missions);
    }

    [HttpPut("missions/{id}")]
    public async Task<IActionResult> UpdateMission(int id, [FromBody] MissionUpdateDto update)
    {
        try
        {
            await _missionManagementService.UpdateMissionAsync(id, update);
            return Ok(new { message = "Mission updated successfully" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("missions/pending")]
    public async Task<IActionResult> GetPendingMissions()
    {
        var missions = await _missionManagementService.GetPendingMissionsAsync();
        return Ok(missions);
    }

    [HttpPost("missions/{id}/approve")]
    public async Task<IActionResult> ApproveMission(int id)
    {
        try
        {
            await _missionManagementService.ApproveMissionAsync(id);
            return Ok(new { message = "Mission approved successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class RejectionDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    [HttpPost("missions/{id}/reject")]
    public async Task<IActionResult> RejectMission(int id, [FromBody] RejectionDto body)
    {
        try
        {
            await _missionManagementService.RejectMissionAsync(id, body.Reason);
            return Ok(new { message = "Mission rejected successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
