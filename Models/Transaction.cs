namespace MasterCard.Models;
public class Transaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ApplicationUser? User { get; set; }
}