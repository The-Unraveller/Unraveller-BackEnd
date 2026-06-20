using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<IEnumerable<ShopItemDto>> GetShopItemsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        bool isPremium = user?.IsPremium ?? false;

        var items = await _shopRepository.GetAllItemsAsync();
        return items.Select(i => new ShopItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Description = i.Description,
            Type = i.Type.ToString(),
            PriceXp = i.PriceXp,
            DiscountPriceXp = i.DiscountPriceXp > 0 ? i.DiscountPriceXp : (isPremium ? (int)(i.PriceXp * 0.8) : i.PriceXp),
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

        int actualPrice = user.IsPremium ? (int)(item.PriceXp * 0.8) : item.PriceXp;

        if (user.XpBalance < actualPrice)
        {
            return new BuyItemResponseDto { Success = false, Message = "Not enough XP balance" };
        }

        user.XpBalance -= actualPrice;
        int quantityToAdd = request.Quantity > 0 ? request.Quantity : 1;
        int currentQuantity = await _shopRepository.GetItemQuantityAsync(userId, item.Id);
        await _shopRepository.UpdateItemQuantityAsync(userId, item.Id, currentQuantity + quantityToAdd);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new BuyItemResponseDto
        {
            Success = true,
            Message = $"Successfully bought {item.Name}!",
            NewXpBalance = user.XpBalance,
            NewQuantity = currentQuantity + quantityToAdd
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
            ItemType.InGameHint => "Hint activated: AI suggestion generated.",
            ItemType.BribeNpc => "Helper used: Communicative drift decreased by 20!",
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

    public async Task<ShopItemDto> CreateShopItemAsync(ShopItemCreateDto dto)
    {
        var item = new ShopItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            PriceXp = dto.PriceXp,
            DiscountPriceXp = dto.DiscountPriceXp,
            Emoji = dto.Emoji
        };

        _shopRepository.Add(item);
        await _shopRepository.SaveChangesAsync();

        return new ShopItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Type = item.Type.ToString(),
            PriceXp = item.PriceXp,
            DiscountPriceXp = item.DiscountPriceXp,
            Emoji = item.Emoji
        };
    }

    public async Task<bool> UpdateShopItemAsync(int id, ShopItemUpdateDto dto)
    {
        var item = await _shopRepository.GetByIdAsync(id);
        if (item == null) return false;

        if (!string.IsNullOrEmpty(dto.Name)) item.Name = dto.Name;
        if (dto.Description != null) item.Description = dto.Description;
        if (dto.Type.HasValue) item.Type = dto.Type.Value;
        if (dto.PriceXp.HasValue) item.PriceXp = dto.PriceXp.Value;
        if (dto.DiscountPriceXp.HasValue) item.DiscountPriceXp = dto.DiscountPriceXp.Value;
        if (!string.IsNullOrEmpty(dto.Emoji)) item.Emoji = dto.Emoji;

        _shopRepository.Update(item);
        await _shopRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteShopItemAsync(int id)
    {
        var item = await _shopRepository.GetByIdAsync(id);
        if (item == null) return false;

        _shopRepository.Delete(item);
        await _shopRepository.SaveChangesAsync();
        return true;
    }
}
