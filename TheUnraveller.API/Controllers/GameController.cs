using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

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

    [HttpPost("message")]
    public async Task<ActionResult<DialogueResponseDto>> SendMessage([FromBody] DialogueRequestDto request)
    {
        try
        {
            var result = await _gameService.ProcessPlayerMessageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
