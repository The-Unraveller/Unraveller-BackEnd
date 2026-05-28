using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameEngineService _gameService;

    public GameController(IGameEngineService gameService)
    {
        _gameService = gameService;
    }

    [Authorize]
    [HttpPost("message")]
    public async Task<ActionResult<DialogueResponseDto>> SendMessage([FromBody] DialogueRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User ID not found in token");
            int userId = int.Parse(userIdClaim.Value);

            var secureRequest = request with { UserId = userId };
            var result = await _gameService.ProcessPlayerMessageAsync(secureRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
