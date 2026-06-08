namespace TheUnraveller.Service.DTOs;

public class ShopItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int PriceXp { get; set; }
    public int DiscountPriceXp { get; set; }
    public string Emoji { get; set; } = "📦";
}

public class BuyItemRequestDto
{
    public int ItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class BuyItemResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int NewXpBalance { get; set; }
    public int NewQuantity { get; set; }
}

public class UseItemRequestDto
{
    public int ItemId { get; set; }
    public int MissionId { get; set; }
}

public class UseItemResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserInventoryDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Emoji { get; set; } = "📦";
}
