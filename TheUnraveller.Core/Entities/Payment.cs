using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
public int PlanId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string? PaymentUrl { get; set; }

    public string? OrderId { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
