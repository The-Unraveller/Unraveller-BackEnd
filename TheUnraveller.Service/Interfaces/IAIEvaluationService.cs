using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface IAIEvaluationService
{
    Task<DialogueResponseWithScoresDto> EvaluateMessageAsync(int userId, int missionId, string playerMessage);
    Task<string> GenerateHintAsync(int userId, int missionId);
    Task<GameSessionDto> GetActiveSessionAsync(int userId, int missionId);
    Task<bool> ResetSessionAsync(int userId, int missionId);
    Task<(bool IsAccessible, string Message)> CheckMissionAccessAsync(int userId, int missionId);
    Task<SkillMapDto> GetWritingSkillMapAsync(int userId);
}
