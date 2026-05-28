using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMissionRepository _missionRepository;

    public AdminController(IUserRepository userRepository, IMissionRepository missionRepository)
    {
        _userRepository = userRepository;
        _missionRepository = missionRepository;
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
        var missions = await _missionRepository.GetAllAsync();
        return Ok(missions);
    }

    [HttpPut("missions/{id}")]
    public async Task<IActionResult> UpdateMission(int id, [FromBody] MissionUpdateDto update)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null) return NotFound();

        if (!string.IsNullOrEmpty(update.Title)) mission.Title = update.Title;
        if (!string.IsNullOrEmpty(update.Goal)) mission.Goal = update.Goal;
        if (!string.IsNullOrEmpty(update.Description)) mission.Description = update.Description;
        if (update.XpReward.HasValue) mission.XpReward = update.XpReward.Value;

        await _missionRepository.UpdateAsync(mission);
        await _missionRepository.SaveChangesAsync();
        return Ok(new { message = "Mission updated successfully" });
    }
}
