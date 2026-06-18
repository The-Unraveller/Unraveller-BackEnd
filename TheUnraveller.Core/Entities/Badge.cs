using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public class Badge
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; } // Emoji or icon name

    // Optional: criteria type for automatic awarding logic
    public string? CriteriaType { get; set; }

    // Optional: required count or threshold
    public int? RequiredCount { get; set; }

    // Optional: associated skill axis for skill-based badges
    public SkillAxis? SkillAxis { get; set; }

    // Optional: minimum average score threshold
    public int? MinAverageScore { get; set; }
}
