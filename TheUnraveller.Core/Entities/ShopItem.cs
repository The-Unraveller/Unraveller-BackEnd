using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public enum ItemType
{
    InGameHint = 1, // Gợi ý câu trả lời tự động trong màn chơi
    BribeNpc = 2, // Giảm độ nghi ngờ lập tức
    Cosmetic = 3 // Khung avatar hoặc huy hiệu trang trí
}

public class ShopItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public ItemType Type { get; set; }

    [Required]
    public int PriceXp { get; set; }

    public int DiscountPriceXp { get; set; }

    public string Emoji { get; set; } = "📦";
}
