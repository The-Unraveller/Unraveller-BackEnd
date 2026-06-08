using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;

namespace TheUnraveller.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
}

public class MissionRepository : GenericRepository<Mission>, IMissionRepository
{
    public MissionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Mission>> GetAvailableMissionsAsync() =>
        await _dbSet.Include(m => m.Npc)
                    .Where(m => m.ApprovalStatus == ApprovalStatus.Approved)
                    .ToListAsync();

    public override async Task<Mission?> GetByIdAsync(int id) =>
        await _dbSet.Include(m => m.Npc).FirstOrDefaultAsync(m => m.Id == id);
}

public class UserProgressRepository : GenericRepository<UserProgress>, IUserProgressRepository
{
    public UserProgressRepository(AppDbContext context) : base(context) { }

    public async Task<UserProgress?> GetUserProgressAsync(int userId, int missionId) =>
        await _dbSet.FirstOrDefaultAsync(up => up.UserId == userId && up.MissionId == missionId);

    public async Task<IEnumerable<UserProgress>> GetUserProgressesAsync(int userId) =>
        await _dbSet.Where(up => up.UserId == userId).ToListAsync();

    public async Task<UserProgress?> GetUserProgressByTokenAsync(string token) =>
        await _dbSet.Include(up => up.User)
                    .Include(up => up.Mission)
                    .ThenInclude(m => m.Npc)
                    .FirstOrDefaultAsync(up => up.CompletionToken == token);
}

public class DialogueRepository : GenericRepository<Dialogue>, IDialogueRepository
{
    public DialogueRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Dialogue>> GetConversationHistoryAsync(int userId, int missionId) =>
        await _dbSet.Where(d => d.UserId == userId && d.MissionId == missionId)
                    .OrderBy(d => d.Timestamp)
                    .ToListAsync();
}

