using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface IAIEvaluationService
{
    Task<DialogueResponseDto> EvaluateMessageAsync(int userId, int missionId, string playerMessage);
    Task<string> GenerateHintAsync(int userId, int missionId);
}
