namespace TheUnraveller.Core.Entities;

public class Dialogue
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int NpcId { get; set; }
    public Npc Npc { get; set; } = null!;

    public int MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public string PlayerMessage { get; set; } = string.Empty;
    public string NpcResponse { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty; // AI explanation (grammar/style) - legacy field

    // Writing coach skill scores (0-100, nullable for old dialogues)
    public int? GrammarScore { get; set; }
    public int? VocabularyScore { get; set; }
    public int? ToneScore { get; set; }
    public int? NaturalnessScore { get; set; }
    public int? ClarityScore { get; set; }
    public int? StructureScore { get; set; }

    public int SuspicionChange { get; set; } // + or - based on the interaction
    public DateTime Timestamp { get; set; }
}
