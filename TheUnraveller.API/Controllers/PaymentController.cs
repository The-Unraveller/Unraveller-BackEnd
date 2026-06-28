using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PayOS.Models.Webhooks;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Core.Entities;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
	private readonly IPaymentService _paymentService;
	private readonly ISubscriptionService _subscriptionService;
	private readonly ILogger<PaymentController> _logger;

	public PaymentController(IPaymentService paymentService, ISubscriptionService subscriptionService, ILogger<PaymentController> logger)
	{
		_paymentService = paymentService;
		_subscriptionService = subscriptionService;
		_logger = logger;
	}

	private int GetUserId()
	{
		var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
		if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
		return int.Parse(userIdClaim.Value);
	}

	[HttpGet("plans")]
	public async Task<IActionResult> GetSubscriptionPlans()
	{
		try
		{
			var plans = await _subscriptionService.GetPlansAsync();
			return Ok(plans);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching subscription plans");
			return StatusCode(500, new { message = "Internal server error" });
		}
	}

	[Authorize]
	[HttpPost("create-payos-link")]
	public async Task<ActionResult<CreatePayOSLinkResponseDto>> CreatePayOSLink([FromBody] CreatePaymentRequestDto request)
	{
		if (!ModelState.IsValid)
			return BadRequest(new CreatePayOSLinkResponseDto { Success = false, Message = "Invalid request data" });

		try
		{
			var userId = GetUserId();
			var result = await _paymentService.CreatePayOSLinkAsync(userId, request.PlanId, (int)request.Amount);

			return result.Success ? Ok(result) : StatusCode(500, result);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating payOS payment link");
			return StatusCode(500, new CreatePayOSLinkResponseDto { Success = false, Message = "Internal server error" });
		}
	}

	[HttpPost("payos-webhook")]
	public async Task<IActionResult> PayOSWebhook([FromBody] Webhook webhookPayload)
	{
		var isValid = await _paymentService.VerifyPayOSWebhookAsync(webhookPayload);

		if (isValid)
			return Ok(new { message = "Payment processed successfully" });

		return BadRequest(new { message = "Invalid or already-processed webhook" });
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

	[Authorize]
	[HttpGet("subscription-status")]
	public async Task<ActionResult<SubscriptionStatusDto>> GetSubscriptionStatus()
	{
		try
		{
			var userId = GetUserId();
			var status = await _subscriptionService.GetUserSubscriptionStatusAsync(userId);
			return Ok(status);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching subscription status");
			return StatusCode(500, new { message = "Internal server error" });
		}
	}

	[Authorize]
	[HttpPost("cancel-subscription")]
	public async Task<ActionResult<object>> CancelSubscription()
	{
		try
		{
			var userId = GetUserId();

			var activeSub = await _subscriptionService.GetActiveSubscriptionAsync(userId);
			if (activeSub == null)
			{
				return BadRequest(new { message = "Không tìm thấy gói đăng ký đang hoạt động" });
			}

			var result = await _subscriptionService.CancelSubscriptionAsync(userId, activeSub.Id);
			if (!result)
			{
				return StatusCode(500, new { message = "Không thể hủy gói đăng ký" });
			}

			return Ok(new { message = "Đã hủy gói đăng ký thành công. Bạn sẽ tiếp tục sử dụng Premium đến hết hạn." });
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cancelling subscription");
			return StatusCode(500, new { message = "Internal server error" });
		}
	}
}
