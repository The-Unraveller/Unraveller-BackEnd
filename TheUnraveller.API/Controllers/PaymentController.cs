using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim.Value);
    }

    [Authorize]
    [HttpPost("create-vnpay-url")]
    public async Task<ActionResult<PaymentResponseDto>> CreateVnpayUrl([FromBody] CreatePaymentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new PaymentResponseDto
            {
                Success = false,
                Message = "Invalid request data"
            });
        }

        var result = await _paymentService.CreatePaymentAsync(request);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return StatusCode(500, result);
        }
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<PaymentHistoryDto>>> GetPaymentHistory()
    {
        try
        {
            var userId = GetUserId();
            var history = await _paymentService.GetPaymentHistoryAsync(userId);
            return Ok(history);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("vnpay-ipn")]
    public async Task<IActionResult> VnpayIpn([FromForm] IFormCollection vnpayData)
    {
        // Sử dụng hashSecret từ config
        string hashSecret = "YOUR_VNPAY_HASH_SECRET";

        var dataDict = vnpayData.ToDictionary(k => k.Key, v => v.Value.ToString());
        bool isValid = await _paymentService.VerifyAndProcessVnpayIPNAsync(dataDict, hashSecret);

        if (isValid)
        {
            return Ok(new { Message = "Payment processed successfully" });
        }
        else
        {
            return BadRequest(new { Message = "Invalid IPN request" });
        }
    }
}
