namespace TheUnraveller.Service.DTOs;

public class UserUpdateDto
{
    public int? XpBalance { get; set; }
    public int? Energy { get; set; }
    public int? Role { get; set; } // Using int for UserRole enum
    public bool? IsPremium { get; set; }
}

public class MissionUpdateDto
{
    public string? Title { get; set; }
    public string? Goal { get; set; }
    public string? Description { get; set; }
    public int? XpReward { get; set; }
}

public class MissionCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StartSuspicion { get; set; } = 10;
    public int MaxSuspicion { get; set; } = 100;
    public string Stage { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int XpReward { get; set; } = 0;
    public string ImageUrl { get; set; } = string.Empty;
    public int NpcId { get; set; } = 1;
}

public class MissionManagementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StartSuspicion { get; set; }
    public int MaxSuspicion { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool Locked { get; set; }
    public int NpcId { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public string NpcEmoji { get; set; } = string.Empty;
    public int ApprovalStatus { get; set; }
    public string? RejectionReason { get; set; }
    public int? CreatedByUserId { get; set; }
}
