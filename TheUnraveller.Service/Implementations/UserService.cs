using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        // 1. Áp dụng Lazy Recharge Energy
        RechargeEnergyLazy(user);

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
            CreatedAt = user.CreatedAt
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

    private void RechargeEnergyLazy(User user)
    {
        var now = DateTime.UtcNow;
        var timeElapsed = now - user.LastEnergyRechargedAt;

        if (timeElapsed.TotalMinutes >= 30)
        {
            int intervals = (int)(timeElapsed.TotalMinutes / 30);
            int energyToRecharge = intervals * 10;

            user.Energy = Math.Min(user.MaxEnergy, user.Energy + energyToRecharge);
            // Cập nhật mốc thời gian vừa hồi xong (không dùng now để tránh mất lẻ phút)
            user.LastEnergyRechargedAt = user.LastEnergyRechargedAt.AddMinutes(intervals * 30);

            // Lưu vào DB
            _userRepository.Update(user);
            _userRepository.SaveChangesAsync().Wait(); // Sử dụng async/await trong method chính
        }
    }
}
