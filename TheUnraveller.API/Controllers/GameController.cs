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
    private readonly IAIEvaluationService _aiEvaluationService;

    public GameController(IAIEvaluationService aiEvaluationService)
    {
        _aiEvaluationService = aiEvaluationService;
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

            var result = await _aiEvaluationService.EvaluateMessageAsync(userId, request.MissionId, request.Message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
