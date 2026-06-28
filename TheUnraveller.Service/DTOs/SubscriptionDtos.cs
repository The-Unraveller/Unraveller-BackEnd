namespace TheUnraveller.Service.DTOs;

public class SubscriptionStatusDto
{
    public bool IsActive { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int DaysRemaining { get; set; } // -1 = lifetime, 0 = expired
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpiringSoon { get; set; } // true if <= 7 days remaining
}
