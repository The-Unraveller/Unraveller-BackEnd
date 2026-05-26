using System.ComponentModel.DataAnnotations;

namespace TheUnraveller.Core.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- CÁC TRƯỜNG THÊM MỚI THEO ROADMAP ---
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;
    public DateTime LastEnergyRechargedAt { get; set; } = DateTime.UtcNow;

    public int StreakCount { get; set; } = 0;
    public DateTime? LastActiveDate { get; set; } // Lưu ngày dưới dạng YYYY-MM-DD

    public int XpBalance { get; set; } = 0; // Điểm XP khả dụng để tiêu dùng trong shop
    public bool IsPremium { get; set; } = false; // Trạng thái tài khoản VIP

    public ICollection<UserProgress> Progresses { get; set; } = new List<UserProgress>();
}
