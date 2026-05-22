using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<IEnumerable<PaymentHistoryDto>>> GetPaymentHistory(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        var history = await _paymentService.GetPaymentHistoryAsync(userId);
        return Ok(history);
    }
}
