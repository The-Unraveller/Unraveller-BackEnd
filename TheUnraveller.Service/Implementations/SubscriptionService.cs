using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(AppDbContext context, ILogger<SubscriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SubscriptionPlan>> GetPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Where(p => p.DurationDays >= 0)
            .OrderBy(p => p.Tier)
            .ToListAsync();
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(int userId)
    {
        return await _context.UserSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<SubscriptionStatusDto?> GetUserSubscriptionStatusAsync(int userId)
    {
        var subscription = await GetActiveSubscriptionAsync(userId);
        if (subscription == null)
        {
            return new SubscriptionStatusDto
            {
                IsActive = false,
                PlanName = "Free",
                DaysRemaining = 0,
                ExpiresAt = null,
                IsExpiringSoon = false
            };
        }

        // Check if subscription has expired
        if (subscription.EndDate.HasValue && subscription.EndDate.Value < DateTime.UtcNow)
        {
            subscription.IsActive = false;
            await _context.SaveChangesAsync();

            // Downgrade user to free
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.IsPremium)
            {
                user.IsPremium = false;
                user.MaxEnergy = 100; // Reset to free tier max energy
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} subscription expired, downgraded to Free", userId);
            }

            return new SubscriptionStatusDto
            {
                IsActive = false,
                PlanName = subscription.Plan?.Name ?? "Unknown",
                DaysRemaining = 0,
                ExpiresAt = subscription.EndDate,
                IsExpiringSoon = false
            };
        }

        int daysRemaining = subscription.EndDate.HasValue
            ? (int)Math.Ceiling((subscription.EndDate.Value - DateTime.UtcNow).TotalDays)
            : -1; // -1 = lifetime

        return new SubscriptionStatusDto
        {
            IsActive = true,
            PlanName = subscription.Plan?.Name ?? "Unknown",
            DaysRemaining = daysRemaining,
            ExpiresAt = subscription.EndDate,
            IsExpiringSoon = daysRemaining > 0 && daysRemaining <= 7
        };
    }

    public async Task<bool> ActivateSubscriptionAsync(int userId, int planId, string transactionId)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        var user = await _context.Users.FindAsync(userId);

        if (plan == null || user == null)
        {
            _logger.LogWarning("ActivateSubscription failed: Plan or User not found. PlanId={PlanId}, UserId={UserId}", planId, userId);
            return false;
        }

        // Calculate end date based on plan duration
        DateTime? endDate = plan.DurationDays > 0
            ? DateTime.UtcNow.AddDays(plan.DurationDays)
            : null; // Lifetime

        // Deactivate any existing active subscriptions for this user
        var existingSubs = await _context.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
        foreach (var sub in existingSubs)
        {
            sub.IsActive = false;
        }

        // Create new subscription
        var subscription = new UserSubscription
        {
            UserId = userId,
            PlanId = planId,
            StartDate = DateTime.UtcNow,
            EndDate = endDate,
            IsActive = true,
            TransactionId = transactionId
        };

        _context.UserSubscriptions.Add(subscription);

        // Update user to premium
        user.IsPremium = true;

        // Set max energy based on plan
        if (plan.DurationDays == 0)
        {
            // Lifetime premium
            user.MaxEnergy = 200;
        }
        else
        {
            user.MaxEnergy = 200;
        }
        user.Energy = user.MaxEnergy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} activated subscription PlanId={PlanId}, expires={EndDate}",
            userId, planId, endDate?.ToString("yyyy-MM-dd") ?? "Never");

        return true;
    }

    public async Task<bool> IsUserPremiumAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId);
        if (sub == null) return false;

        // If has end date and expired, deactivate
        if (sub.EndDate.HasValue && sub.EndDate.Value < DateTime.UtcNow)
        {
            sub.IsActive = false;
            await _context.SaveChangesAsync();
            return false;
        }

        // Lifetime (EndDate == null) or not yet expired
        return true;
    }

    /// <summary>
    /// Find all subscriptions expiring within the specified number of days.
    /// Used for sending expiration notifications.
    /// </summary>
    public async Task<List<UserSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry = 7)
    {
        var threshold = DateTime.UtcNow.AddDays(daysBeforeExpiry);

        return await _context.UserSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.User)
            .Where(s => s.IsActive
                     && s.EndDate.HasValue
                     && s.EndDate.Value <= threshold
                     && s.EndDate.Value > DateTime.UtcNow)
            .ToListAsync();
    }

    /// <summary>
    /// Deactivate all expired subscriptions and downgrade users to free tier.
    /// Should be called by a scheduled job/cron.
    /// </summary>
    public async Task<int> ProcessExpiredSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredSubs = await _context.UserSubscriptions
            .Include(s => s.User)
            .Where(s => s.IsActive && s.EndDate.HasValue && s.EndDate.Value < now)
            .ToListAsync();

        int count = 0;
        foreach (var sub in expiredSubs)
        {
            sub.IsActive = false;
            if (sub.User != null)
            {
                sub.User.IsPremium = false;
                sub.User.MaxEnergy = 100;
                count++;
                _logger.LogInformation("Subscription expired for UserId={UserId}, PlanId={PlanId}",
                    sub.UserId, sub.PlanId);
            }
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return count;
    }

    public async Task<bool> CancelSubscriptionAsync(int userId, int subscriptionId)
    {
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null) return false;

        subscription.IsActive = false;
        await _context.SaveChangesAsync();

        // Don't immediately downgrade — let user keep premium until end date
        _logger.LogInformation("User {UserId} cancelled subscription {SubscriptionId}", userId, subscriptionId);

        return true;
    }
}
