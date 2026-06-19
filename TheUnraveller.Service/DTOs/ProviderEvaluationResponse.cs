using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.DTOs;

/// <summary>
/// Response from LLM provider containing evaluation data for a single player message.
/// This is the normalized contract between AIEvaluationService and the LLM provider.
/// </summary>
public class ProviderEvaluationResponse
{
    /// <summary>
    /// NPC's response to the player (2-3 sentences in English)
    /// </summary>
    public string NpcResponse { get; set; } = string.Empty;

    /// <summary>
    /// Structured writing feedback including scores and corrections
    /// </summary>
    public WritingFeedbackDto WritingFeedback { get; set; } = new(
        new WritingScoreDto(50, 50, 50, 50, 50, 50),
        new List<CorrectionDto>(),
        null,
        "* No feedback available"
    );

    /// <summary>
    /// Change in suspicion level (negative = good, positive = bad)
    /// </summary>
    public int SuspicionChange { get; set; }

    /// <summary>
    /// XP earned for this turn (0-20)
    /// </summary>
    public int XpEarned { get; set; }
}
