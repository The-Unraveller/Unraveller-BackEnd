using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Web;
using Microsoft.EntityFrameworkCore;
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

    public PaymentService(IPaymentRepository paymentRepository, IUserRepository userRepository, AppDbContext context)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request)
    {
        try
        {
            var payment = new Payment
            {
                UserId = request.UserId,
                PlanId = request.PlanId,
                Amount = request.Amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

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
            return new PaymentResponseDto { Success = false, Message = $"Failed: {ex.Message}" };
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

    public async Task<bool> VerifyAndProcessVnpayIPNAsync(IDictionary<string, string> vnpayData, string hashSecret)
    {
        try
        {
            string vnpSecureHash = vnpayData.ContainsKey("vnp_SecureHash") ? vnpayData["vnp_SecureHash"] : string.Empty;

            var sortedParams = vnpayData
                .Where(kvp => kvp.Key.StartsWith("vnp_") && kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .OrderBy(kvp => kvp.Key)
                .ToList();

            StringBuilder signData = new StringBuilder();
            foreach (var kvp in sortedParams)
            {
                signData.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
            }
            if (signData.Length > 0) signData.Length--;

            string myChecksum = ComputeHmacSha512(hashSecret, signData.ToString());

            if (myChecksum.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase))
            {
                string responseCode = vnpayData.ContainsKey("vnp_ResponseCode") ? vnpayData["vnp_ResponseCode"] : string.Empty;
                string orderIdStr = vnpayData.ContainsKey("vnp_TxnRef") ? vnpayData["vnp_TxnRef"] : string.Empty;

                if (responseCode == "00")
                {
                    int paymentId = int.Parse(orderIdStr.Replace("ORDER_", ""));
                    var payment = await _paymentRepository.GetByIdAsync(paymentId);

                    if (payment != null && payment.Status == "Pending")
                    {
                        payment.Status = "Completed";
                        payment.OrderId = vnpayData.ContainsKey("vnp_TransactionNo") ? vnpayData["vnp_TransactionNo"] : string.Empty;

                        var user = await _userRepository.GetByIdAsync(payment.UserId);
                        if (user != null)
                        {
                            user.IsPremium = true;
                            user.MaxEnergy = 200;
                        }

                        _paymentRepository.Update(payment);
                        await _paymentRepository.SaveChangesAsync();

                        if (user != null)
                        {
                            _userRepository.Update(user);
                            await _userRepository.SaveChangesAsync();
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeHmacSha512(string key, string data)
    {
        var encoding = new ASCIIEncoding();
        byte[] keyByte = encoding.GetBytes(key);
        byte[] messageBytes = encoding.GetBytes(data);
        using var hmacsha512 = new HMACSHA512(keyByte);
        byte[] hashmessage = hmacsha512.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashmessage);
    }
}
