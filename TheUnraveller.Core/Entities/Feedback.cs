using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public class Feedback
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int Rating { get; set; } = 5;

    public string Category { get; set; } = "Trải nghiệm UI/UX";

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
