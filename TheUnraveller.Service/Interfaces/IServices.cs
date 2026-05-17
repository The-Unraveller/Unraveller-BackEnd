using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface IUserService
{
    Task<UserDto?> AuthenticateAsync(string email, string password);
    Task<UserDto> RegisterAsync(string username, string email, string password);
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
