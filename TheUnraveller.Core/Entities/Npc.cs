namespace TheUnraveller.Core.Entities;

public class Npc
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // e.g., "Security Guard", "Hacker"
    public string Description { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty; // For AI context
}
