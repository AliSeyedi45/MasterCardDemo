namespace MasterCard.DTOs;
public class ChargeRequestResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}