using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class ShopService : IShopService
{
    private readonly IShopRepository _shopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserProgressRepository _userProgressRepository;

    public ShopService(
        IShopRepository shopRepository, 
        IUserRepository userRepository,
        IUserProgressRepository userProgressRepository)
    {
        _shopRepository = shopRepository;
        _userRepository = userRepository;
        _userProgressRepository = userProgressRepository;
    }

    public async Task<IEnumerable<ShopItemDto>> GetShopItemsAsync()
    {
        var items = await _shopRepository.GetAllItemsAsync();
        return items.Select(i => new ShopItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Description = i.Description,
            Type = i.Type.ToString(),
            PriceXp = i.PriceXp,
            Emoji = i.Emoji
        });
    }

    public async Task<BuyItemResponseDto> BuyItemAsync(int userId, BuyItemRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var item = await _shopRepository.GetByIdAsync(request.ItemId);

        if (user == null || item == null)
        {
            return new BuyItemResponseDto { Success = false, Message = "User or Item not found" };
        }

        if (user.XpBalance < item.PriceXp)
        {
            return new BuyItemResponseDto { Success = false, Message = "Not enough XP balance" };
        }

        user.XpBalance -= item.PriceXp;
        int currentQuantity = await _shopRepository.GetItemQuantityAsync(userId, item.Id);
        await _shopRepository.UpdateItemQuantityAsync(userId, item.Id, currentQuantity + 1);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new BuyItemResponseDto
        {
            Success = true,
            Message = $"Successfully bought {item.Name}!",
            NewXpBalance = user.XpBalance,
            NewQuantity = currentQuantity + 1
        };
    }

    public async Task<UseItemResponseDto> UseItemAsync(int userId, UseItemRequestDto request)
    {
        int quantity = await _shopRepository.GetItemQuantityAsync(userId, request.ItemId);
        if (quantity <= 0)
        {
            return new UseItemResponseDto { Success = false, Message = "You don't have this item" };
        }

        var item = await _shopRepository.GetByIdAsync(request.ItemId);
        if (item == null) return new UseItemResponseDto { Success = false, Message = "Item not found" };

        await _shopRepository.UpdateItemQuantityAsync(userId, request.ItemId, quantity - 1);

        string effectMessage = item.Type switch
        {
            ItemType.InGameHint => "Hint activated: NPC is now more likely to give a clue. Suspicion decreased by 10!",
            ItemType.BribeNpc => "Bribe successful: Suspicion level decreased by 20!",
            ItemType.Cosmetic => "Cosmetic applied to your profile.",
            _ => "Item used successfully."
        };

        if (request.MissionId > 0)
        {
            var progress = await _userProgressRepository.GetUserProgressAsync(userId, request.MissionId);
            if (progress != null && progress.Status == MissionStatus.InProgress)
            {
                if (item.Type == ItemType.BribeNpc)
                {
                    progress.CurrentSuspicion = Math.Max(0, progress.CurrentSuspicion - 20);
                    _userProgressRepository.Update(progress);
                    await _userProgressRepository.SaveChangesAsync();
                }
                else if (item.Type == ItemType.InGameHint)
                {
                    progress.CurrentSuspicion = Math.Max(0, progress.CurrentSuspicion - 10);
                    _userProgressRepository.Update(progress);
                    await _userProgressRepository.SaveChangesAsync();
                }
            }
        }

        return new UseItemResponseDto
        {
            Success = true,
            Message = effectMessage
        };
    }

    public async Task<IEnumerable<UserInventoryDto>> GetUserInventoryAsync(int userId)
    {
        var inventory = await _shopRepository.GetUserInventoryAsync(userId);
        return inventory.Select(ui => new UserInventoryDto
        {
            ItemId = ui.ItemId,
            Name = ui.Item?.Name ?? "Unknown Item",
            Description = ui.Item?.Description ?? string.Empty,
            Type = ui.Item?.Type.ToString() ?? string.Empty,
            Quantity = ui.Quantity,
            Emoji = ui.Item?.Emoji ?? "📦"
        });
    }
}
