using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using SkillAxisEntity = TheUnraveller.Core.Entities.SkillAxis;

namespace TheUnraveller.Service.Implementations;

public class ProgressService : IProgressService
{
    private readonly AppDbContext _context;

    public ProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SkillMapDto> GetSkillMapAsync(int userId)
    {
        // Get last 5 completed scenario snapshots
        var snapshots = await _context.WritingSkillSnapshots
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CompletedAt)
            .Take(5)
            .ToListAsync();

        if (!snapshots.Any())
        {
            return new SkillMapDto(
                new WritingScoreDto(0, 0, 0, 0, 0, 0),
                new Dictionary<string, decimal>()
            );
        }

        var currentAvg = new WritingScoreDto(
            (int)Math.Round(snapshots.Average(s => s.GrammarScore)),
            (int)Math.Round(snapshots.Average(s => s.VocabularyScore)),
            (int)Math.Round(snapshots.Average(s => s.ToneScore)),
            (int)Math.Round(snapshots.Average(s => s.NaturalnessScore)),
            (int)Math.Round(snapshots.Average(s => s.ClarityScore)),
            (int)Math.Round(snapshots.Average(s => s.StructureScore))
        );

        // Build historical trend: date → average score
        var trend = snapshots
            .GroupBy(s => s.CompletedAt.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key.ToString("yyyy-MM-dd"),
                g => Math.Round(g.Average(s => s.AverageScore), 2)
            );

        return new SkillMapDto(currentAvg, trend);
    }

    public async Task<List<PortfolioEntryDto>> GetPortfolioAsync(int userId)
    {
        var snapshots = await _context.WritingSkillSnapshots
            .Include(s => s.Mission)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync();

        var entries = snapshots.Select(s => new PortfolioEntryDto(
            s.MissionId,
            s.Mission.Title,
            s.Mission.Domain.ToString(),
            s.Mission.CefrLevel.ToString(),
            s.CompletedAt,
            new WritingScoreDto(
                s.GrammarScore,
                s.VocabularyScore,
                s.ToneScore,
                s.NaturalnessScore,
                s.ClarityScore,
                s.StructureScore
            ),
            s.TurnsCount,
            s.Mission.XpReward // Note: this is static mission reward, not actual earned XP
        )).ToList();

        return entries;
    }

    public async Task<WeeklyReportDto> GetWeeklyReportAsync(int userId)
    {
        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

        // Get snapshots from last 7 days
        var recentSnapshots = await _context.WritingSkillSnapshots
            .Include(s => s.Mission)
            .Where(s => s.UserId == userId && s.CompletedAt >= oneWeekAgo)
            .ToListAsync();

        var weekStart = DateTime.UtcNow.Date.AddDays(-7);

        // Average score across all recent completions
        decimal avgScore = recentSnapshots.Any()
            ? Math.Round(recentSnapshots.Average(s => s.AverageScore), 2)
            : 0;

        // Scenarios completed this week
        int completedCount = recentSnapshots.Count;

        // Top error types: analyze corrections from dialogues in these missions
        var missionIds = recentSnapshots.Select(s => s.MissionId).ToList();
        var errorCounts = new Dictionary<SkillAxisEntity, int>();

        var corrections = await _context.Corrections
            .Include(c => c.Dialogue)
            .Where(c => missionIds.Contains(c.Dialogue.MissionId) && c.Dialogue.UserId == userId)
            .ToListAsync();

        foreach (var correction in corrections)
        {
            var axis = correction.Axis; // This is Core.Entities.SkillAxis
            if (errorCounts.ContainsKey(axis))
                errorCounts[axis]++;
            else
                errorCounts[axis] = 1;
        }

        var topErrorTypes = errorCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .Select(kvp => kvp.Key.ToString())
            .ToList();

        // New vocabulary count: estimate from unique words in corrections (simplified)
        // For now, return 0 as placeholder - would require NLP analysis
        int newVocabCount = 0;

        // Recommended scenarios: missions not yet completed with similar or slightly higher CEFR
        var completedMissionIds = await _context.UserProgresses
            .Where(p => p.UserId == userId && p.Status == MissionStatus.Completed)
            .Select(p => p.MissionId)
            .ToListAsync();

        var recommended = await _context.Missions
            .Where(m => !completedMissionIds.Contains(m.Id) &&
                        m.ApprovalStatus == ApprovalStatus.Approved &&
                        m.Domain == DomainType.Professional) // Default to Professional for now
            .OrderBy(m => m.CefrLevel)
            .Take(3)
            .Select(m => m.Id)
            .ToListAsync();

        return new WeeklyReportDto(
            weekStart,
            avgScore,
            completedCount,
            topErrorTypes,
            newVocabCount,
            recommended
        );
    }
}
