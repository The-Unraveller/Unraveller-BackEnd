using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly ISubscriptionService _subscriptionService;

    public UserService(
        IUserRepository userRepository,
        IUserProgressRepository userProgressRepository,
        ISubscriptionService subscriptionService)
    {
        _userRepository = userRepository;
        _userProgressRepository = userProgressRepository;
        _subscriptionService = subscriptionService;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        // 1. Apply Lazy Recharge Energy
        await RechargeEnergyLazyAsync(user);

        var progresses = await _userProgressRepository.GetUserProgressesAsync(userId);

        // 2. Get subscription status
        var subStatus = await _subscriptionService.GetUserSubscriptionStatusAsync(userId);

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
            SubscriptionPlanName = subStatus?.PlanName,
            SubscriptionEndDate = subStatus?.ExpiresAt,
            SubscriptionDaysRemaining = subStatus?.DaysRemaining ?? 0,
            SubscriptionExpiringSoon = subStatus?.IsExpiringSoon ?? false,
            CreatedAt = user.CreatedAt,
            MissionProgresses = progresses.Select(p => new UserMissionProgressDto
            {
                MissionId = p.MissionId,
                CurrentSuspicion = p.CurrentSuspicion,
                Status = p.Status.ToString(),
                TurnCount = p.TurnCount,
                XpEarned = p.XpEarned,
                CompletionToken = p.CompletionToken,
                CompletedAt = p.CompletedAt
            }).ToList()
        };
    }

    public async Task UpdateStreakAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var today = DateTime.UtcNow.Date;
        var lastActive = user.LastActiveDate?.Date ?? DateTime.MinValue;

        if (lastActive < today.AddDays(-1))
        {
            // Streak broken
            user.StreakCount = 1;
        }
        else if (lastActive < today)
        {
            // Consecutive day
            user.StreakCount++;
        }

        user.LastActiveDate = DateTime.UtcNow;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateEnglishLevelAsync(int userId, string englishLevel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        user.EnglishLevel = englishLevel;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateProfileAsync(int userId, string username, string email)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        user.Username = username;
        user.Email = email;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    private async Task RechargeEnergyLazyAsync(User user)
    {
        var now = DateTime.UtcNow;
        var timeElapsed = now - user.LastEnergyRechargedAt;

        if (timeElapsed.TotalMinutes >= 30)
        {
            int intervals = (int)(timeElapsed.TotalMinutes / 30);
            int energyPerInterval = user.IsPremium ? 20 : 10;
            int energyToRecharge = intervals * energyPerInterval;

            user.Energy = Math.Min(user.MaxEnergy, user.Energy + energyToRecharge);
            user.LastEnergyRechargedAt = user.LastEnergyRechargedAt.AddMinutes(intervals * 30);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
