using System.Text.Json.Serialization;

namespace TheUnraveller.Service.DTOs;

public record WritingScoreDto(
    [property: JsonPropertyName("grammar")] int Grammar,
    [property: JsonPropertyName("vocabulary")] int Vocabulary,
    [property: JsonPropertyName("tone")] int Tone,
    [property: JsonPropertyName("naturalness")] int Naturalness,
    [property: JsonPropertyName("clarity")] int Clarity,
    [property: JsonPropertyName("structure")] int Structure
);

public record CorrectionDto(
    [property: JsonPropertyName("axis")] SkillAxis Axis,
    [property: JsonPropertyName("original")] string OriginalText,
    [property: JsonPropertyName("corrected")] string CorrectedText,
    [property: JsonPropertyName("explanation")] string Explanation
);

public record WritingFeedbackDto(
    [property: JsonPropertyName("scores")] WritingScoreDto Scores,
    [property: JsonPropertyName("corrections")] List<CorrectionDto> Corrections,
    [property: JsonPropertyName("rewriteSuggestion")] string? RewriteSuggestion,
    [property: JsonPropertyName("summary")] string Summary
);

public record DialogueResponseWithScoresDto(
    [property: JsonPropertyName("npcResponse")] string NpcResponse,
    [property: JsonPropertyName("writingFeedback")] WritingFeedbackDto WritingFeedback,
    [property: JsonPropertyName("newSuspicionLevel")] int NewSuspicionLevel,
    [property: JsonPropertyName("isWin")] bool IsWin,
    [property: JsonPropertyName("isLose")] bool IsLose,
    [property: JsonPropertyName("turnCount")] int TurnCount,
    [property: JsonPropertyName("xpEarned")] int XpEarned,
    [property: JsonPropertyName("completionToken")] string? CompletionToken = null,
    [property: JsonPropertyName("updatedEnergy")] int? UpdatedEnergy = null,
    [property: JsonPropertyName("updatedMaxEnergy")] int? UpdatedMaxEnergy = null
);

public record SkillMapDto(
    [property: JsonPropertyName("currentAverage")] WritingScoreDto CurrentAverage,
    [property: JsonPropertyName("historicalTrend")] Dictionary<string, decimal> HistoricalTrend
);

public record PortfolioEntryDto(
    [property: JsonPropertyName("missionId")] int MissionId,
    [property: JsonPropertyName("missionTitle")] string MissionTitle,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("cefrLevel")] string CefrLevel,
    [property: JsonPropertyName("completedAt")] DateTime CompletedAt,
    [property: JsonPropertyName("finalScores")] WritingScoreDto FinalScores,
    [property: JsonPropertyName("turnsCount")] int TurnsCount,
    [property: JsonPropertyName("totalXp")] int TotalXp
);

public record WeeklyReportDto(
    [property: JsonPropertyName("weekStartDate")] DateTime WeekStartDate,
    [property: JsonPropertyName("averageScore")] decimal AverageScore,
    [property: JsonPropertyName("scenariosCompleted")] int ScenariosCompleted,
    [property: JsonPropertyName("topErrorTypes")] List<string> TopErrorTypes,
    [property: JsonPropertyName("newVocabularyCount")] int NewVocabularyCount,
    [property: JsonPropertyName("recommendedScenarios")] List<int> RecommendedScenarioIds
);

// Existing enums for DTO usage (already defined in Core.Entities but expose for serialization)
public enum SkillAxis
{
    Grammar = 0,
    Vocabulary = 1,
    Tone = 2,
    Naturalness = 3,
    Clarity = 4,
    Structure = 5
}
