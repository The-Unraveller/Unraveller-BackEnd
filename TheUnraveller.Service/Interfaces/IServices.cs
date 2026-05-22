using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId);
}

public interface IMissionService
{
    Task<IEnumerable<MissionDto>> GetAllMissionsAsync();
    Task<MissionDto?> GetMissionByIdAsync(int id);
}

public interface IGameEngineService
{
    Task<DialogueResponseDto> ProcessPlayerMessageAsync(DialogueRequestDto request);
}

public interface ILLMProviderService
{
    Task<LlmResponseDto> GetNpcResponseAsync(string systemPrompt, string userMessage);
}

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request);
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(int userId);
}
