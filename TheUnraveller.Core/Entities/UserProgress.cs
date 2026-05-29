namespace TheUnraveller.Core.Entities;

public enum MissionStatus
{
    InProgress,
    Completed,
    Failed
}

public class UserProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int MissionId { get; set; }
    public Mission Mission { get; set; } = null!;
    
    public int CurrentSuspicion { get; set; } // Current level on "Thanh Nghi Ngờ"
    public MissionStatus Status { get; set; } = MissionStatus.InProgress;
    
    public int TurnCount { get; set; } = 0; // Tracks game progression
    public int XpEarned { get; set; } = 0;
    
    public DateTime LastActivity { get; set; }
}
