using TheUnraveller.Service.DTOs;

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
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request);
    Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(int userId);
    Task<bool> VerifyAndProcessVnpayIPNAsync(IDictionary<string, string> vnpayData, string hashSecret);
}

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task UpdateStreakAsync(int userId);
}

public interface IShopService
{
    Task<IEnumerable<ShopItemDto>> GetShopItemsAsync();
    Task<BuyItemResponseDto> BuyItemAsync(int userId, BuyItemRequestDto request);
    Task<UseItemResponseDto> UseItemAsync(int userId, UseItemRequestDto request);
}
