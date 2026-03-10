namespace TheUnraveller.Core.Entities;

public class Mission
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StartSuspicion { get; set; } = 0; // Default: 0
    public int MaxSuspicion { get; set; } = 100; // Threshold to "Lose"
    
    public int NpcId { get; set; }
    public Npc Npc { get; set; } = null!;
}
