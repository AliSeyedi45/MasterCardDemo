using MasterCard.Data;
using MasterCard.DTOs;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Services;
public class TransactionService
{
    private readonly ApplicationDbContext _context;
    public TransactionService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<TransactionResponse>>
        GetMyTransactionsAsync(string userId)
    {
        return await _context.Transactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TransactionResponse
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Amount = x.Amount,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }
    public async Task<TransactionResponse?>
        GetTransactionByIdAsync(int transactionId, string userId)
    {
        return await _context.Transactions
            .Where(x =>
                x.Id == transactionId &&
                x.UserId == userId)
            .Select(x => new TransactionResponse
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Amount = x.Amount,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }
}
