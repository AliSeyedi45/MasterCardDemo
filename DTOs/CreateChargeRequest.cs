namespace MasterCard.DTOs;
public class CreateChargeRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}