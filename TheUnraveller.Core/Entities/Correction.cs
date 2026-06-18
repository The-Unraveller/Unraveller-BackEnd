namespace TheUnraveller.Core.Entities;

public class Correction
{
    public int Id { get; set; }
    public int DialogueId { get; set; }
    public Dialogue Dialogue { get; set; } = null!;

    public SkillAxis Axis { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
