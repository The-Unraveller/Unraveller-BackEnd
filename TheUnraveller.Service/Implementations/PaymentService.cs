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
        _returnUrl = configuration["PayOS:ReturnUrl"] ?? "http://localhost:5173/premium?payment=success";
        _cancelUrl = configuration["PayOS:CancelUrl"] ?? "http://localhost:5173/premium?payment=failed";
    }

    /// <summary>
    /// Creates a payOS hosted checkout link and persists a Pending payment record.
    /// </summary>
    public async Task<CreatePayOSLinkResponseDto> CreatePayOSLinkAsync(int userId, string planId, int amount)
    {
        try
        {
            // Generate a unique numeric order code required by payOS (must be a positive integer)
            // Using last 9 digits of Unix-millisecond timestamp — unique enough for a university project
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1_000_000_000L;

            // Persist pending payment record first so we can match on webhook
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

            // Build the payOS payment request
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

    /// <summary>
    /// Verifies the payOS webhook HMAC signature and upgrades user to Premium on success.
    /// </summary>
    public async Task<bool> VerifyPayOSWebhookAsync(Webhook webhookPayload)
    {
        try
        {
            // SDK verifies the HMAC-SHA256 signature using ChecksumKey internally
            // If the signature is invalid, it will throw an exception.
            var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookPayload);

            // Once the signature is verified, we should return true (200 OK) to payOS
            // to confirm receipt of the authenticated webhook, even if the order was already completed.
            if (webhookPayload.Code == "00" && webhookPayload.Success)
            {
                var orderCode = webhookData.OrderCode.ToString();

                // If it is a test/verification order code (123), return true immediately
                if (webhookData.OrderCode == 123)
                {
                    return true;
                }

                // Find the matching Pending payment record
                var payment = await _context.Set<Payment>()
                    .FirstOrDefaultAsync(p => p.OrderId == orderCode && p.Status == "Pending");

                if (payment != null)
                {
                    // Mark payment as completed
                    payment.Status = "Completed";
                    payment.CompletedAt = DateTime.UtcNow;
                    _paymentRepository.Update(payment);
                    await _paymentRepository.SaveChangesAsync();

                    // Upgrade user to Premium
                    var user = await _userRepository.GetByIdAsync(payment.UserId);
                    if (user != null)
                    {
                        user.IsPremium = true;
                        user.MaxEnergy = 200;
                        user.Energy = 200; // Recharge current energy to full
                        _userRepository.Update(user);
                        await _userRepository.SaveChangesAsync();
                    }
                }
            }

            return true;
        }
        catch
        {
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

            if (pendingPayments.Any())
            {
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
                                Console.WriteLine($"[SyncPendingPayments] OrderCode {orderCode} status on PayOS: {statusStr}");
                                if (statusStr.Equals("PAID", StringComparison.OrdinalIgnoreCase))
                                {
                                    payment.Status = "Completed";
                                    payment.CompletedAt = DateTime.UtcNow;
                                    _paymentRepository.Update(payment);
                                    hasUpdates = true;

                                    // Upgrade user to Premium
                                    var user = await _userRepository.GetByIdAsync(userId);
                                    if (user != null)
                                    {
                                        user.IsPremium = true;
                                        user.MaxEnergy = 200;
                                        user.Energy = 200; // Recharge current energy to full
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
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                if (hasUpdates)
                {
                    await _paymentRepository.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SyncPendingPayments] Outer error: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Returns the payment history for a given user.
    /// </summary>
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
