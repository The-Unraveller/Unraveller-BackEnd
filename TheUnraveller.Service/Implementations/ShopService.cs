using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class ShopService : IShopService
{
    private readonly IShopRepository _shopRepository;
    private readonly IUserRepository _userRepository;

    public ShopService(IShopRepository shopRepository, IUserRepository userRepository)
    {
        _shopRepository = shopRepository;
        _userRepository = userRepository;
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
            ItemType.InGameHint => "Hint activated: NPC is now more likely to give a clue.",
            ItemType.BribeNpc => "Bribe successful: Suspicion level decreased!",
            ItemType.Cosmetic => "Cosmetic applied to your profile.",
            _ => "Item used successfully."
        };

        return new UseItemResponseDto
        {
            Success = true,
            Message = effectMessage
        };
    }
}
