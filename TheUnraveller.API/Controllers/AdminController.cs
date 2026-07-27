using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMissionManagementService _missionManagementService;
    private readonly IShopService _shopService;
    private readonly AppDbContext _context;

    public AdminController(
        IUserRepository userRepository, 
        IMissionManagementService missionManagementService, 
        IShopService shopService,
        AppDbContext context)
    {
        _userRepository = userRepository;
        _missionManagementService = missionManagementService;
        _shopService = shopService;
        _context = context;
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var freeUsers = await _context.Users.CountAsync(u => !u.IsPremium);
        var paidUsers = await _context.Users.CountAsync(u => u.IsPremium);
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == "Completed")
            .SumAsync(p => (long)p.Amount);
        var completedTransactionsCount = await _context.Payments.CountAsync(p => p.Status == "Completed");
        var totalTransactionsCount = await _context.Payments.CountAsync();

        var conversionRate = totalUsers > 0 ? Math.Round((double)paidUsers / totalUsers * 100, 1) : 0;

        var levelGroup = await _context.Users
            .GroupBy(u => u.EnglishLevel ?? "B1")
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            totalUsers,
            freeUsers,
            paidUsers,
            totalRevenue,
            completedTransactionsCount,
            totalTransactionsCount,
            conversionRate,
            funnel = new
            {
                awareness = Math.Max(totalUsers * 4, 150), // Traffic / visit metric for marketing funnel
                leads = totalUsers,
                freeUsers = freeUsers,
                paidUsers = paidUsers
            },
            englishLevels = levelGroup
        });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await (from p in _context.Payments
                                  join u in _context.Users on p.UserId equals u.Id into userGroup
                                  from user in userGroup.DefaultIfEmpty()
                                  orderby p.CreatedAt descending
                                  select new
                                  {
                                      p.Id,
                                      p.OrderId,
                                      p.UserId,
                                      Username = user != null ? user.Username : "N/A",
                                      UserEmail = user != null ? user.Email : "N/A",
                                      p.PlanId,
                                      p.Amount,
                                      p.Status,
                                      p.CreatedAt,
                                      p.CompletedAt
                                  }).ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users.Select(u => new {
            u.Id, u.Username, u.Email, Role = u.Role.ToString(), u.XpBalance, u.Energy, u.IsPremium, u.EnglishLevel
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
        if (!string.IsNullOrEmpty(update.EnglishLevel)) user.EnglishLevel = update.EnglishLevel;

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

    // ── Shop Items CRUD ──

    [HttpGet("shop-items")]
    public async Task<IActionResult> GetAllShopItems()
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
