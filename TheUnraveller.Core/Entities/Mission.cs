using TheUnraveller.Core.Entities;

namespace TheUnraveller.Core.Entities;

public enum ApprovalStatus
{
    Approved = 0,
    Pending = 1,
    Rejected = 2
}

public class Mission
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WritingObjective { get; set; } = string.Empty; // Specific writing task for this scenario
    public DomainType Domain { get; set; } = DomainType.Professional; // Professional, Academic, Social
    public CefrLevel CefrLevel { get; set; } = CefrLevel.B1; // Language difficulty level

    public int StartSuspicion { get; set; } = 0; // Default: 0
    public int MaxSuspicion { get; set; } = 100; // Threshold to "Lose"
    public int MinTurnsToComplete { get; set; } = 5; // Minimum turns to complete scenario
    public int MinAverageScore { get; set; } = 70; // Minimum average writing score to complete

    // Rendering & Session Metadata
    public string Stage { get; set; } = string.Empty; // e.g., "Stage 1"
    public string Difficulty { get; set; } = string.Empty; // e.g., "Beginner"
    public int XpReward { get; set; } = 0;
    public string ImageUrl { get; set; } = string.Empty;
    public bool Locked { get; set; } = false;
    public string GrammarTarget { get; set; } = string.Empty; // e.g., "Sử dụng câu điều kiện loại 1 (If...)"
    public List<string> InitialChoices { get; set; } = new();
    public string SyntaxPuzzlesJson { get; set; } = "[]";

    public int NpcId { get; set; }
    public Npc Npc { get; set; } = null!;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;
    public string? RejectionReason { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
