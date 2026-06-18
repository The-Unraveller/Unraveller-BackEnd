namespace TheUnraveller.Core.Entities;

public class WritingSkillSnapshot
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public decimal AverageScore { get; set; } // Mean of 6 axes, 0-100
    public int GrammarScore { get; set; }
    public int VocabularyScore { get; set; }
    public int ToneScore { get; set; }
    public int NaturalnessScore { get; set; }
    public int ClarityScore { get; set; }
    public int StructureScore { get; set; }

    public int TurnsCount { get; set; }
    public DateTime CompletedAt { get; set; }

    // Optional highlights
    public string? BestSentence { get; set; }
    public string? AiRewriteSuggestion { get; set; }
}
