using Microsoft.EntityFrameworkCore;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _context;

    public LeaderboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId)
    {
        var users = await _context.Users
            .Include(u => u.Progresses)
            .ToListAsync();

        var rankedUsers = users
            .Select(u => new
            {
                u.Id,
                Name = u.Username,
                TotalXp = u.Progresses.Sum(p => p.XpEarned)
            })
            .OrderByDescending(x => x.TotalXp)
            .ToList();

        var leaderboard = new List<LeaderboardEntryDto>();
        for (int i = 0; i < rankedUsers.Count; i++)
        {
            var user = rankedUsers[i];
            int rank = i + 1;
            string badge = rank switch
            {
                1 => "👑",
                2 => "🥈",
                3 => "🥉",
                _ => "⚡"
            };

            leaderboard.Add(new LeaderboardEntryDto(
                rank,
                user.Name,
                user.TotalXp,
                badge,
                user.Id == currentUserId
            ));
        }

        return leaderboard;
    }
}
