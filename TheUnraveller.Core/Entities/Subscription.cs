using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public enum SubscriptionTier
{
    Free = 0,
    MonthlyPremium = 1,
    YearlyPremium = 2,
    LifetimePremium = 3
}

public class SubscriptionPlan
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SubscriptionTier Tier { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; } // 0 for lifetime
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
}

public class UserSubscription
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string TransactionId { get; set; } = string.Empty;

    // Navigation properties
    public User? User { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}
