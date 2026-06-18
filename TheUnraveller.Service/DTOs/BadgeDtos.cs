using System.Text.Json.Serialization;

namespace TheUnraveller.Service.DTOs;

public class BadgeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}

public class UserBadgeDto
{
    [JsonPropertyName("badgeId")]
    public int BadgeId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("earnedAt")]
    public DateTime EarnedAt { get; set; }
}
