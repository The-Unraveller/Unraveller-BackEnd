using TheUnraveller.Core.Entities;
using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlan>> GetPlansAsync();
    Task<UserSubscription?> GetActiveSubscriptionAsync(int userId);
    Task<SubscriptionStatusDto?> GetUserSubscriptionStatusAsync(int userId);
    Task<bool> ActivateSubscriptionAsync(int userId, int planId, string transactionId);
    Task<bool> IsUserPremiumAsync(int userId);
    Task<List<UserSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry = 7);
    Task<int> ProcessExpiredSubscriptionsAsync();
    Task<bool> CancelSubscriptionAsync(int userId, int subscriptionId);
}
