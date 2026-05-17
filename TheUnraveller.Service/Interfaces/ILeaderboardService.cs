using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId);
}
