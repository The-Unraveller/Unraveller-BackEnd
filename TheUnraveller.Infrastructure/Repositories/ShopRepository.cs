using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;

namespace TheUnraveller.Infrastructure.Repositories;

public class ShopRepository : GenericRepository<ShopItem>, IShopRepository
{
    public ShopRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ShopItem>> GetAllItemsAsync() =>
        await _dbSet.ToListAsync();

    public async Task<int> GetItemQuantityAsync(int userId, int itemId)
    {
        var inventory = await _context.Set<UserInventory>()
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

        return inventory?.Quantity ?? 0;
    }

    public async Task UpdateItemQuantityAsync(int userId, int itemId, int quantity)
    {
        var inventory = await _context.Set<UserInventory>()
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

        if (inventory == null)
        {
            _context.Set<UserInventory>().Add(new UserInventory
            {
                UserId = userId,
                ItemId = itemId,
                Quantity = quantity
            });
        }
        else
        {
            inventory.Quantity = quantity;
        }

        await SaveChangesAsync();
    }

    public async Task<IEnumerable<UserInventory>> GetUserInventoryAsync(int userId)
    {
        return await _context.Set<UserInventory>()
            .Include(ui => ui.Item)
            .Where(ui => ui.UserId == userId && ui.Quantity > 0)
            .ToListAsync();
    }
}
