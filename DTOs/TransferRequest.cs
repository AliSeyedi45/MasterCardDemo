namespace MasterCard.DTOs;
public class TransferRequest
{
    public string ReceiverUserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}