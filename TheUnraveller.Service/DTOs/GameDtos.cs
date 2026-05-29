namespace TheUnraveller.Service.DTOs;

public record UserDto(int Id, string Username, string Email);

public record MissionDto(
    int Id, 
    string Title, 
    string Goal, 
    string Description, 
    int StartSuspicion,
    string Stage,
    string Difficulty,
    int XpReward,
    string ImageUrl,
    string NpcName,
    string NpcEmoji,
    bool Locked
);

public record DialogueRequestDto(int UserId, int MissionId, string Message);

public record DialogueResponseDto(
    string NpcResponse, 
    string Feedback, 
    int NewSuspicionLevel, 
    bool IsWin, 
    bool IsLose,
    int TurnCount,
    int XpEarned
);

public class LlmResponseDto
{
    public string NpcResponse { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public int SuspicionDelta { get; set; }
}

public record LeaderboardEntryDto(int Rank, string Name, int Xp, string Badge, bool IsYou);

public record UseGameItemRequestDto(int ItemId, int MissionId);

public record UseGameItemResponseDto(
    bool Success,
    string Message,
    int NewSuspicionLevel,
    string? Hint
);
