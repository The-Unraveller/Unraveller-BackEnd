using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProgressRepository _userProgressRepository;

    public UserService(IUserRepository userRepository, IUserProgressRepository userProgressRepository)
    {
        _userRepository = userRepository;
        _userProgressRepository = userProgressRepository;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        // 1. Áp dụng Lazy Recharge Energy
        RechargeEnergyLazy(user);

        var progresses = await _userProgressRepository.GetUserProgressesAsync(userId);

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Energy = user.Energy,
            MaxEnergy = user.MaxEnergy,
            LastEnergyRechargedAt = user.LastEnergyRechargedAt,
            StreakCount = user.StreakCount,
            LastActiveDate = user.LastActiveDate,
            XpBalance = user.XpBalance,
            IsPremium = user.IsPremium,
            EnglishLevel = user.EnglishLevel,
            CreatedAt = user.CreatedAt,
            MissionProgresses = progresses.Select(p => new UserMissionProgressDto
            {
                MissionId = p.MissionId,
                CurrentSuspicion = p.CurrentSuspicion,
                Status = p.Status.ToString(),
                TurnCount = p.TurnCount,
                XpEarned = p.XpEarned
            }).ToList()
        };
    }

    public async Task UpdateStreakAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        // 2. Logic Daily Streak (UTC+7)
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var todayVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).Date;

        if (user.LastActiveDate == null)
        {
            user.StreakCount = 1;
        }
        else
        {
            var lastActiveVn = TimeZoneInfo.ConvertTimeFromUtc(user.LastActiveDate.Value, vnTimeZone).Date;
            var diffDays = (todayVn - lastActiveVn).Days;

            if (diffDays == 1)
            {
                user.StreakCount += 1;
            }
            else if (diffDays > 1)
            {
                user.StreakCount = 1; // Reset về 1 nếu đứt chuỗi
            }
            // Nếu diffDays == 0, không làm gì (vẫn trong ngày hôm nay)
        }

        user.LastActiveDate = DateTime.UtcNow;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateEnglishLevelAsync(int userId, string englishLevel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var level = englishLevel?.Trim().ToUpper() ?? "B1";
        if (level != "A1" && level != "A2" && level != "B1" && level != "B2" && level != "C1" && level != "C2")
        {
            throw new ArgumentException("Trình độ không hợp lệ. Phải thuộc một trong các cấp độ: A1, A2, B1, B2, C1, C2.");
        }

        user.EnglishLevel = level;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateProfileAsync(int userId, string username, string email)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var cleanUsername = username?.Trim();
        var cleanEmail = email?.Trim();

        if (string.IsNullOrWhiteSpace(cleanUsername)) throw new ArgumentException("Tên người dùng không được để trống.");
        if (string.IsNullOrWhiteSpace(cleanEmail)) throw new ArgumentException("Email không được để trống.");

        // Check unique username
        var existingUsername = await _userRepository.GetByUsernameAsync(cleanUsername);
        if (existingUsername != null && existingUsername.Id != userId)
        {
            throw new InvalidOperationException("Tên người dùng đã được sử dụng.");
        }

        // Check unique email
        var existingEmail = await _userRepository.GetByEmailAsync(cleanEmail);
        if (existingEmail != null && existingEmail.Id != userId)
        {
            throw new InvalidOperationException("Email đã được sử dụng.");
        }

        user.Username = cleanUsername;
        user.Email = cleanEmail;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    private void RechargeEnergyLazy(User user)
    {
        var now = DateTime.UtcNow;
        var timeElapsed = now - user.LastEnergyRechargedAt;

        if (timeElapsed.TotalMinutes >= 30)
        {
            int intervals = (int)(timeElapsed.TotalMinutes / 30);
            int energyToRecharge = intervals * (user.IsPremium ? 20 : 10);

            user.Energy = Math.Min(user.MaxEnergy, user.Energy + energyToRecharge);
            // Cập nhật mốc thời gian vừa hồi xong (không dùng now để tránh mất lẻ phút)
            user.LastEnergyRechargedAt = user.LastEnergyRechargedAt.AddMinutes(intervals * 30);

            // Lưu vào DB
            _userRepository.Update(user);
            _userRepository.SaveChangesAsync().Wait(); // Sử dụng async/await trong method chính
        }
    }
}
