using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;

    public SubscriptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionPlan>> GetPlansAsync()
    {
        return await _context.SubscriptionPlans.ToListAsync();
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ActivateSubscriptionAsync(string userId, int planId, string transactionId)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (plan == null || user == null) return false;

        // Calculate end date
        DateTime? endDate = plan.DurationDays > 0
            ? DateTime.UtcNow.AddDays(plan.DurationDays)
            : null;

        var subscription = new UserSubscription
        {
            UserId = userId,
            PlanId = planId,
            StartDate = DateTime.UtcNow,
            EndDate = endDate,
            IsActive = true,
            TransactionId = transactionId
        };

        user.IsPremium = true;

        _context.UserSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsUserPremiumAsync(string userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId);
        if (sub == null) return false;

        // If Lifetime (EndDate == null) or not yet expired
        return sub.EndDate == null || sub.EndDate > DateTime.UtcNow;
    }
}
