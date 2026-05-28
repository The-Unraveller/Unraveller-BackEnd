namespace TheUnraveller.Service.DTOs;

public class CreatePaymentRequestDto
{
    public int UserId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// Legacy DTO — kept for history endpoint
public class PaymentResponseDto
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? OrderId { get; set; }
    public string? Message { get; set; }
}

// payOS checkout link response
public class CreatePayOSLinkResponseDto
{
    public bool Success { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? Message { get; set; }
}

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? OrderId { get; set; }
}
