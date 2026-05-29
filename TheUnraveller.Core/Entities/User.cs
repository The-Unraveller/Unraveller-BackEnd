using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; }

    // --- CÁC TRƯỜNG THÊM MỚI THEO ROADMAP ---
    public int Energy { get; set; } = 100;
    public int MaxEnergy { get; set; } = 100;
    public DateTime LastEnergyRechargedAt { get; set; }

    public int StreakCount { get; set; } = 0;
    public DateTime? LastActiveDate { get; set; } // REMOVED TEMPORARILY

    public int XpBalance { get; set; } = 0; // Điểm XP khả dụng để tiêu dùng trong shop
    public bool IsPremium { get; set; } = false; // Trạng thái tài khoản VIP
    public string EnglishLevel { get; set; } = "B1"; // Trình độ Tiếng Anh thích ứng (CEFR)

    public ICollection<UserProgress> Progresses { get; set; } = new List<UserProgress>();
}
