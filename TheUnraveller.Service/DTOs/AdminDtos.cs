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
