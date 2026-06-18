using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface IBadgeService
{
    /// <summary>
    /// Awards badges to the user based on mission completion and stats.
    /// Call this within a transaction after mission win to include badge awards in the same transaction.
    /// </summary>
    Task AwardBadgesForMissionAsync(int userId, int missionId, decimal averageScore, CancellationToken cancellationToken = default);

    Task<List<UserBadgeDto>> GetUserBadgesAsync(int userId, CancellationToken cancellationToken = default);
}
