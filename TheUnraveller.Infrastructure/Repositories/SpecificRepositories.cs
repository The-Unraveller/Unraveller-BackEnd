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
        await _dbSet.Include(m => m.Npc).ToListAsync();

    public override async Task<Mission?> GetByIdAsync(int id) =>
        await _dbSet.Include(m => m.Npc).FirstOrDefaultAsync(m => m.Id == id);
}

public class UserProgressRepository : GenericRepository<UserProgress>, IUserProgressRepository
{
    public UserProgressRepository(AppDbContext context) : base(context) { }

    public async Task<UserProgress?> GetUserProgressAsync(int userId, int missionId) =>
        await _dbSet.FirstOrDefaultAsync(up => up.UserId == userId && up.MissionId == missionId);
}

public class DialogueRepository : GenericRepository<Dialogue>, IDialogueRepository
{
    public DialogueRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Dialogue>> GetConversationHistoryAsync(int userId, int missionId) =>
        await _dbSet.Where(d => d.UserId == userId && d.MissionId == missionId)
                    .OrderBy(d => d.Timestamp)
                    .ToListAsync();
}

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId) =>
        await _dbSet.Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
}

public class ShopRepository : GenericRepository<ShopItem>, IShopRepository
{
    private readonly AppDbContext _ctx;

    public ShopRepository(AppDbContext context) : base(context)
    {
        _ctx = context;
    }

    public async Task<IEnumerable<ShopItem>> GetAllItemsAsync() =>
        await _dbSet.ToListAsync();

    public async Task<int> GetItemQuantityAsync(int userId, int itemId)
    {
        var inv = await _ctx.Set<UserInventory>()
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);
        return inv?.Quantity ?? 0;
    }

    public async Task UpdateItemQuantityAsync(int userId, int itemId, int quantity)
    {
        var inv = await _ctx.Set<UserInventory>()
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

        if (inv == null)
        {
            inv = new UserInventory { UserId = userId, ItemId = itemId, Quantity = quantity };
            await _ctx.Set<UserInventory>().AddAsync(inv);
        }
        else
        {
            inv.Quantity = quantity;
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserInventory>> GetUserInventoryAsync(int userId) =>
        await _ctx.Set<UserInventory>()
            .Include(ui => ui.Item)
            .Where(ui => ui.UserId == userId && ui.Quantity > 0)
            .ToListAsync();
}
