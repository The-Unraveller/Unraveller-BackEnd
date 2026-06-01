using TheUnraveller.Service.DTOs;
using PayOS.Models.Webhooks;

namespace TheUnraveller.Service.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId);
}

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string username, string email, string password);
    Task<string> LoginWithGoogleAsync(string idToken);
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
    Task<CreatePayOSLinkResponseDto> CreatePayOSLinkAsync(int userId, string planId, int amount);
    Task<bool> VerifyPayOSWebhookAsync(Webhook webhookPayload);
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(int userId);
}

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task UpdateStreakAsync(int userId);
    Task UpdateEnglishLevelAsync(int userId, string englishLevel);
    Task UpdateProfileAsync(int userId, string username, string email);
}

public interface IShopService
{
    Task<IEnumerable<ShopItemDto>> GetShopItemsAsync();
    Task<BuyItemResponseDto> BuyItemAsync(int userId, BuyItemRequestDto request);
    Task<UseItemResponseDto> UseItemAsync(int userId, UseItemRequestDto request);
    Task<IEnumerable<UserInventoryDto>> GetUserInventoryAsync(int userId);
}

public interface IMissionManagementService
{
    Task<IEnumerable<MissionManagementDto>> GetAllMissionsForManagementAsync();
    Task<IEnumerable<MissionManagementDto>> GetPendingMissionsAsync();
    Task<bool> CreateMissionAsync(MissionCreateDto dto, int creatorId);
    Task<bool> UpdateMissionAsync(int id, MissionUpdateDto dto);
    Task<bool> ApproveMissionAsync(int id);
    Task<bool> RejectMissionAsync(int id, string reason);
}
