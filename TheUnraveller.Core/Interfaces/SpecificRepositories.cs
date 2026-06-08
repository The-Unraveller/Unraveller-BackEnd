using TheUnraveller.Core.Entities;

namespace TheUnraveller.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
}

public interface IMissionRepository : IGenericRepository<Mission>
{
    Task<IEnumerable<Mission>> GetAvailableMissionsAsync();
}

public interface IUserProgressRepository : IGenericRepository<UserProgress>
{
    Task<UserProgress?> GetUserProgressAsync(int userId, int missionId);
    Task<IEnumerable<UserProgress>> GetUserProgressesAsync(int userId);
    Task<UserProgress?> GetUserProgressByTokenAsync(string token);
}

public interface IDialogueRepository : IGenericRepository<Dialogue>
{
    Task<IEnumerable<Dialogue>> GetConversationHistoryAsync(int userId, int missionId);
}

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
}

public interface IShopRepository : IGenericRepository<ShopItem>
{
    Task<IEnumerable<ShopItem>> GetAllItemsAsync();
    Task<int> GetItemQuantityAsync(int userId, int itemId);
    Task UpdateItemQuantityAsync(int userId, int itemId, int quantity);
    Task<IEnumerable<UserInventory>> GetUserInventoryAsync(int userId);
}
