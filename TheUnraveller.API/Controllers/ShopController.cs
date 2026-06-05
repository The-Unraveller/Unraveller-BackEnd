using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim.Value);
    }

    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<ShopItemDto>>> GetItems()
    {
        try
        {
            var userId = GetUserId();
            var items = await _shopService.GetShopItemsAsync(userId);
            return Ok(items);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetInventory()
    {
        try
        {
            var userId = GetUserId();
            var result = await _shopService.GetUserInventoryAsync(userId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("buy")]
    public async Task<ActionResult<BuyItemResponseDto>> BuyItem([FromBody] BuyItemRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _shopService.BuyItemAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("use-item")]
    public async Task<ActionResult<UseItemResponseDto>> UseItem([FromBody] UseItemRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _shopService.UseItemAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
