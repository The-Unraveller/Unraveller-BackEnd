using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Infrastructure.Data;

namespace TheUnraveller.Service.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;
    private readonly PayOSClient _payOSClient;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;
    private readonly int _premiumMaxEnergy;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        AppDbContext context,
        PayOSClient payOSClient,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _context = context;
        _payOSClient = payOSClient;
        _returnUrl = configuration["PayOS:ReturnUrl"] ?? throw new InvalidOperationException("PayOS ReturnUrl is not configured");
        _cancelUrl = configuration["PayOS:CancelUrl"] ?? throw new InvalidOperationException("PayOS CancelUrl is not configured");
        _premiumMaxEnergy = configuration.GetValue<int>("GameRules:PremiumMaxEnergy", 200);
    }

    public async Task<CreatePayOSLinkResponseDto> CreatePayOSLinkAsync(int userId, int planId, int amount)
    {
        try
        {
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1_000_000_000L;

            var payment = new Payment
            {
                UserId = userId,
                PlanId = planId,
                Amount = amount,
                Status = "Pending",
                OrderId = orderCode.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = $"Unraveller {planId}",
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

            return new CreatePayOSLinkResponseDto
            {
                Success = true,
                CheckoutUrl = paymentLink.CheckoutUrl
            };
        }
        catch (Exception ex)
        {
            return new CreatePayOSLinkResponseDto
            {
                Success = false,
                Message = $"Failed to create payment link: {ex.Message}"
            };
        }
    }

    public async Task<bool> VerifyPayOSWebhookAsync(Webhook webhookPayload)
    {
        try
        {
            var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookPayload);

            if (webhookPayload.Code == "00" && webhookPayload.Success)
            {
                var orderCode = webhookData.OrderCode.ToString();

                if (webhookData.OrderCode == 123)
                {
                    return true;
                }

                var payment = await _context.Set<Payment>()
                    .FirstOrDefaultAsync(p => p.OrderId == orderCode && p.Status == "Pending");

                if (payment != null)
                {
                    payment.Status = "Completed";
                    payment.CompletedAt = DateTime.UtcNow;
                    _paymentRepository.Update(payment);
                    await _paymentRepository.SaveChangesAsync();

                    var user = await _userRepository.GetByIdAsync(payment.UserId);
                    if (user != null)
                    {
                        user.IsPremium = true;
                        user.MaxEnergy = _premiumMaxEnergy;
                        user.Energy = _premiumMaxEnergy;
                        _userRepository.Update(user);
                        await _userRepository.SaveChangesAsync();
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS Webhook] Verification failed: {ex.Message}");
            return false;
        }
    }

    private async Task SyncPendingPaymentsAsync(int userId)
    {
        try
        {
            var pendingPayments = await _context.Set<Payment>()
                .Where(p => p.UserId == userId && p.Status == "Pending")
                .ToListAsync();

            if (!pendingPayments.Any()) return;

            bool hasUpdates = false;
            foreach (var payment in pendingPayments)
            {
                if (long.TryParse(payment.OrderId, out long orderCode))
                {
                    try
                    {
                        Console.WriteLine($"[SyncPendingPayments] Checking orderCode: {orderCode}");
                        var paymentInfo = await _payOSClient.PaymentRequests.GetAsync(orderCode);
                        if (paymentInfo != null)
                        {
                            string statusStr = paymentInfo.Status.ToString();
                            Console.WriteLine($"[SyncPendingPayments] OrderCode {orderCode} status: {statusStr}");

                            if (statusStr.Equals("PAID", StringComparison.OrdinalIgnoreCase))
                            {
                                payment.Status = "Completed";
                                payment.CompletedAt = DateTime.UtcNow;
                                _paymentRepository.Update(payment);
                                hasUpdates = true;

                                var user = await _userRepository.GetByIdAsync(userId);
                                if (user != null)
                                {
                                    user.IsPremium = true;
                                    user.MaxEnergy = _premiumMaxEnergy;
                                    user.Energy = _premiumMaxEnergy;
                                    _userRepository.Update(user);
                                }
                            }
                            else if (statusStr.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                            {
                                payment.Status = "Failed";
                                _paymentRepository.Update(payment);
                                hasUpdates = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SyncPendingPayments] Error checking orderCode {orderCode}: {ex.Message}");
                    }
                }
            }

            if (hasUpdates)
            {
                await _paymentRepository.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SyncPendingPayments] Outer error: {ex.Message}");
        }
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(int userId)
    {
        await SyncPendingPaymentsAsync(userId);

        var payments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
        return payments.Select(p => new PaymentHistoryDto
        {
            Id = p.Id,
            PlanId = p.PlanId,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            OrderId = p.OrderId
        });
    }
}
