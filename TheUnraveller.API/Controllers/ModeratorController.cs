using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModeratorController : ControllerBase
{
    private readonly IMissionManagementService _missionManagementService;
    private readonly IShopService _shopService;

    public ModeratorController(IMissionManagementService missionManagementService, IShopService shopService)
    {
        _missionManagementService = missionManagementService;
        _shopService = shopService;
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

    // ── Shop Items CRUD (Moderator + Admin) ──

    [HttpGet("shop-items")]
    public async Task<IActionResult> GetShopItems()
    {
        var items = await _shopService.GetShopItemsAsync(0);
        return Ok(items);
    }

    [HttpPost("shop-items")]
    public async Task<IActionResult> CreateShopItem([FromBody] ShopItemCreateDto dto)
    {
        try
        {
            var item = await _shopService.CreateShopItemAsync(dto);
            return Ok(item);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("shop-items/{id}")]
    public async Task<IActionResult> UpdateShopItem(int id, [FromBody] ShopItemUpdateDto dto)
    {
        try
        {
            var success = await _shopService.UpdateShopItemAsync(id, dto);
            if (!success) return NotFound(new { error = "Shop item not found" });
            return Ok(new { message = "Shop item updated successfully" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("shop-items/{id}")]
    public async Task<IActionResult> DeleteShopItem(int id)
    {
        var success = await _shopService.DeleteShopItemAsync(id);
        if (!success) return NotFound(new { error = "Shop item not found" });
        return Ok(new { message = "Shop item deleted successfully" });
    }
}
