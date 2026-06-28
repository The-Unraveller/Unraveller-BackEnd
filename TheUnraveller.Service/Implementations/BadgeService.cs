using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class BadgeService : IBadgeService
{
    private readonly AppDbContext _context;

    public BadgeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AwardBadgesForMissionAsync(int userId, int missionId, decimal averageScore, CancellationToken cancellationToken = default)
    {
        var badges = await _context.Badges.ToListAsync(cancellationToken);
        var earnedBadgeIds = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync(cancellationToken);

        var newBadges = new List<UserBadge>();

        // 1. First Steps: if this is the user's first completed mission
        var firstCompletionBadge = badges.FirstOrDefault(b => b.CriteriaType == "FirstCompletion");
        if (firstCompletionBadge != null && !earnedBadgeIds.Contains(firstCompletionBadge.Id))
        {
            var priorCompletions = await _context.UserProgresses
                .CountAsync(p => p.UserId == userId && p.Status == MissionStatus.Completed && p.MissionId != missionId, cancellationToken);
            if (priorCompletions == 0)
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = firstCompletionBadge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        // 2. MinAverageScore: Skillful (70), Perfectionist (90)
        var scoreBadges = badges.Where(b => b.CriteriaType == "MinAverageScore" && b.MinAverageScore.HasValue && averageScore >= b.MinAverageScore.Value).ToList();
        foreach (var badge in scoreBadges)
        {
            if (!earnedBadgeIds.Contains(badge.Id))
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        // 3. TotalCompletions: Lifetime Learner (10)
        var totalCompletions = await _context.UserProgresses
            .CountAsync(p => p.UserId == userId && p.Status == MissionStatus.Completed, cancellationToken);

        var lifetimeBadges = badges.Where(b => b.CriteriaType == "TotalCompletions" && b.RequiredCount.HasValue && totalCompletions >= b.RequiredCount.Value).ToList();
        foreach (var badge in lifetimeBadges)
        {
            if (!earnedBadgeIds.Contains(badge.Id))
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        // 4. DomainDiversity: Linguist
        var domainBadge = badges.FirstOrDefault(b => b.CriteriaType == "DomainDiversity");
        if (domainBadge != null && !earnedBadgeIds.Contains(domainBadge.Id))
        {
            var completedMissionIds = await _context.UserProgresses
                .Where(p => p.UserId == userId && p.Status == MissionStatus.Completed)
                .Select(p => p.MissionId)
                .ToListAsync(cancellationToken);

            var domains = await _context.Missions
                .Where(m => completedMissionIds.Contains(m.Id))
                .Select(m => m.Domain)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (domains.Count >= 3)
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = domainBadge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        // 5. TotalXp: Writing Coach
        var xpBadge = badges.FirstOrDefault(b => b.CriteriaType == "TotalXp");
        if (xpBadge != null && !earnedBadgeIds.Contains(xpBadge.Id))
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null && user.XpBalance >= xpBadge.RequiredCount.GetValueOrDefault())
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = xpBadge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        // 6. Streak Master: check streak count >= required
        var streakBadge = badges.FirstOrDefault(b => b.CriteriaType == "Streak");
        if (streakBadge != null && !earnedBadgeIds.Contains(streakBadge.Id))
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user != null && user.StreakCount >= streakBadge.RequiredCount.GetValueOrDefault())
            {
                newBadges.Add(new UserBadge { UserId = userId, BadgeId = streakBadge.Id, EarnedAt = DateTime.UtcNow });
            }
        }

        if (newBadges.Any())
        {
            await _context.UserBadges.AddRangeAsync(newBadges, cancellationToken);
        }
    }

    public async Task<List<UserBadgeDto>> GetUserBadgesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var badgesWithDetails = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => new UserBadgeDto
            {
                BadgeId = ub.BadgeId,
                Name = ub.Badge.Name,
                Description = ub.Badge.Description,
                Icon = ub.Badge.Icon,
                EarnedAt = ub.EarnedAt
            })
            .ToListAsync(cancellationToken);

        return badgesWithDetails;
    }
}
