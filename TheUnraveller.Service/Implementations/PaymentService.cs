using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Infrastructure.Data;

namespace TheUnraveller.Service.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly AppDbContext _context;

    public PaymentService(IPaymentRepository paymentRepository, AppDbContext context)
    {
        _paymentRepository = paymentRepository;
        _context = context;
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request)
    {
        try
        {
            // Create payment record
            var payment = new TheUnraveller.Core.Entities.Payment
            {
                UserId = request.UserId,
                PlanId = request.PlanId,
                Amount = request.Amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // For VNPay integration, generate payment URL
            // In a real implementation, this would call VNPay API or generate redirect URL
            var paymentUrl = $"https://vnpay.example.com/pay?orderId={payment.Id}&amount={request.Amount}";

            return new PaymentResponseDto
            {
                Success = true,
                PaymentUrl = paymentUrl,
                OrderId = $"ORDER_{payment.Id}",
                Message = "Payment created successfully"
            };
        }
        catch (Exception ex)
        {
            return new PaymentResponseDto
            {
                Success = false,
                Message = $"Failed to create payment: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(int userId)
    {
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
