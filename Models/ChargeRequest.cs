namespace MasterCard.Models;
public class ChargeRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApplicationUser? User { get; set; }
}