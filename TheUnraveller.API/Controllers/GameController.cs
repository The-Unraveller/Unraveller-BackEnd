using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TheUnraveller.Service.DTOs;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IAIEvaluationService _aiEvaluationService;
    private readonly IShopRepository _shopRepository;
    private readonly IUserProgressRepository _userProgressRepository;

    public GameController(
        IAIEvaluationService aiEvaluationService,
        IShopRepository shopRepository,
        IUserProgressRepository userProgressRepository)
    {
        _aiEvaluationService = aiEvaluationService;
        _shopRepository = shopRepository;
        _userProgressRepository = userProgressRepository;
    }

    [Authorize]
    [HttpPost("message")]
    public async Task<ActionResult<DialogueResponseWithScoresDto>> SendMessage([FromBody] DialogueRequestDto request)
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

    [Authorize]
    [HttpGet("session/{missionId}")]
    public async Task<ActionResult<GameSessionDto>> GetSession(int missionId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User ID not found in token");
            int userId = int.Parse(userIdClaim.Value);

            var result = await _aiEvaluationService.GetActiveSessionAsync(userId, missionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("check-access/{missionId}")]
    public async Task<ActionResult> CheckMissionAccess(int missionId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User ID not found in token");
            int userId = int.Parse(userIdClaim.Value);

            var result = await _aiEvaluationService.CheckMissionAccessAsync(userId, missionId);
            return Ok(new { isAccessible = result.IsAccessible, message = result.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("reset/{missionId}")]
    public async Task<ActionResult> ResetSession(int missionId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User ID not found in token");
            int userId = int.Parse(userIdClaim.Value);

            await _aiEvaluationService.ResetSessionAsync(userId, missionId);
            return Ok(new { message = "Khởi động lại nhiệm vụ thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("use-item")]
    public async Task<ActionResult<UseGameItemResponseDto>> UseItem([FromBody] UseGameItemRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new UseGameItemResponseDto(false, "Không tìm thấy User ID trong token.", 0, null));
            int userId = int.Parse(userIdClaim.Value);

            // 1. Verify item quantity in inventory
            int quantity = await _shopRepository.GetItemQuantityAsync(userId, request.ItemId);
            if (quantity <= 0)
            {
                return BadRequest(new UseGameItemResponseDto(false, "Bạn không sở hữu vật phẩm này trong túi đồ.", 0, null));
            }

            // 2. Decrement item quantity
            await _shopRepository.UpdateItemQuantityAsync(userId, request.ItemId, quantity - 1);

            int newSuspicion = 0;
            string? hint = null;
            string message = "Sử dụng vật phẩm thành công.";

            // 3. Apply item effects
            if (request.ItemId == 2) // Golden Tongue / Lưỡi vàng (BribeNpc)
            {
                var progress = await _userProgressRepository.GetUserProgressAsync(userId, request.MissionId);
                if (progress != null)
                {
                    progress.CurrentSuspicion = Math.Max(0, progress.CurrentSuspicion - 20);
                    progress.LastActivity = DateTime.UtcNow;
                    _userProgressRepository.Update(progress);
                    await _userProgressRepository.SaveChangesAsync();
                    newSuspicion = progress.CurrentSuspicion;
                }
                message = "Đã sử dụng Lưỡi Vàng! Mức độ nghi ngờ của NPC giảm đi 20 điểm.";
            }
            else if (request.ItemId == 1) // Detective Magnifier / Kính lúp thám tử (InGameHint)
            {
                var progress = await _userProgressRepository.GetUserProgressAsync(userId, request.MissionId);
                if (progress != null)
                {
                    newSuspicion = progress.CurrentSuspicion;
                }
                
                // Call LLM hint service to generate a Vietnamese suggestion
                hint = await _aiEvaluationService.GenerateHintAsync(userId, request.MissionId);
                message = "Đã kích hoạt Kính Lúp Thám Tử! AI đã phân tích tình huống và đưa ra gợi ý.";
            }

            return Ok(new UseGameItemResponseDto(true, message, newSuspicion, hint));
        }
        catch (Exception ex)
        {
            return BadRequest(new UseGameItemResponseDto(false, $"Lỗi hệ thống: {ex.Message}", 0, null));
        }
    }

    [HttpGet("certificate/{token}")]
    public async Task<ActionResult> GetCertificateByToken(string token)
    {
        try
        {
            var progress = await _userProgressRepository.GetUserProgressByTokenAsync(token);
            if (progress == null)
            {
                return NotFound(new { error = "Không tìm thấy chứng chỉ tương ứng với mã token này." });
            }

            return Ok(new
            {
                Token = progress.CompletionToken,
                CompletedAt = progress.CompletedAt ?? progress.LastActivity,
                XpEarned = progress.XpEarned,
                TurnCount = progress.TurnCount,
                User = new
                {
                    Username = progress.User.Username,
                    EnglishLevel = progress.User.EnglishLevel
                },
                Mission = new
                {
                    Id = progress.Mission.Id,
                    Title = progress.Mission.Title,
                    Goal = progress.Mission.Goal,
                    GrammarTarget = progress.Mission.GrammarTarget,
                    Stage = progress.Mission.Stage,
                    Difficulty = progress.Mission.Difficulty,
                    NpcName = progress.Mission.Npc?.Name ?? "NPC"
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves the user's Writing Skill Map showing current averages and historical trends
    /// </summary>
    [Authorize]
    [HttpGet("writing-skill-map")]
    public async Task<ActionResult<SkillMapDto>> GetWritingSkillMap()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User ID not found in token");
            int userId = int.Parse(userIdClaim.Value);

            var result = await _aiEvaluationService.GetWritingSkillMapAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
