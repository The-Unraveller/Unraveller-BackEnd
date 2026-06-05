namespace TheUnraveller.Service.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Energy fields
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
    public DateTime LastEnergyRechargedAt { get; set; }

    // Streak fields
    public int StreakCount { get; set; }
    public DateTime? LastActiveDate { get; set; }

    // Shop fields
    public int XpBalance { get; set; }
    public bool IsPremium { get; set; }
    public string EnglishLevel { get; set; } = "B1";

    // Created info
    public DateTime CreatedAt { get; set; }

    public List<UserMissionProgressDto> MissionProgresses { get; set; } = new();
}

public class UserMissionProgressDto
{
    public int MissionId { get; set; }
    public int CurrentSuspicion { get; set; }
    public string Status { get; set; } = string.Empty; // "InProgress", "Completed", "Failed"
    public int TurnCount { get; set; }
    public int XpEarned { get; set; }
}

public class UpdateEnglishLevelRequestDto
{
    public string EnglishLevel { get; set; } = string.Empty;
}

public class UpdateProfileRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
