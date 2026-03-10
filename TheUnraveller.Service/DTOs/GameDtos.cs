namespace TheUnraveller.Service.DTOs;

public record UserDto(int Id, string Username, string Email);

public record MissionDto(int Id, string Title, string Goal, string Description, int StartSuspicion);

public record DialogueRequestDto(int UserId, int MissionId, string Message);

public record DialogueResponseDto(
    string NpcResponse, 
    string Feedback, 
    int NewSuspicionLevel, 
    bool IsWin, 
    bool IsLose
);
