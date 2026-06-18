using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheUnraveller.Core.Entities;

public class UserBadge
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BadgeId { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Badge Badge { get; set; } = null!;
}
