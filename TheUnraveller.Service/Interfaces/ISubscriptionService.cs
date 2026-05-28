using TheUnraveller.Core.Entities;

namespace TheUnraveller.Service.Interfaces;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlan>> GetPlansAsync();
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
    Task<bool> ActivateSubscriptionAsync(string userId, int planId, string transactionId);
    Task<bool> IsUserPremiumAsync(string userId);
}
