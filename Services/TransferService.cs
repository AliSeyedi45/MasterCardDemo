using System.Data;
using MasterCard.Data;
using MasterCard.DTOs;
using MasterCard.Models;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Services;
public class TransferService
{
    private readonly ApplicationDbContext _context;
    public TransferService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<(bool Success, string Message)>
        TransferAsync(
            string senderUserId,
            TransferRequest request)
    {
        if (request.Amount <= 0)
        {
            return (false, "Amount must be greater than zero.");
        }
        if (string.IsNullOrWhiteSpace(request.ReceiverUserId))
        {
            return (false, "Receiver account is required.");
        }
        if (senderUserId == request.ReceiverUserId)
        {
            return (false, "You cannot transfer money to yourself.");
        }
        await using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var sender = await _context.Users.FirstOrDefaultAsync(x => x.Id == senderUserId);
            if (sender == null)
            {
                return (false, "Sender account not found.");
            }
            if (!sender.IsActive)
            {
                return (false, "Sender account is inactive.");
            }
            var receiver = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.ReceiverUserId);
            if (receiver == null)
            {
                return (false, "Receiver account not found.");
            }
            if (!receiver.IsActive)
            {
                return (false, "Receiver account is inactive.");
            }
            if (sender.Balance < request.Amount)
            {
                return (false, "Insufficient balance.");
            }
            var now = DateTime.UtcNow;
            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;
            var outgoingTransaction = new Transaction
            {
                UserId = sender.Id,
                Type = TransactionType.TransferOut,
                Amount = request.Amount,
                Description = string.IsNullOrWhiteSpace(request.Description)? $"Transfer to {receiver.Email}": request.Description,
                CreatedAt = now
            };
            var incomingTransaction = new Transaction
            {
                UserId = receiver.Id,
                Type = TransactionType.TransferIn,
                Amount = request.Amount,
                Description = $"Transfer from {sender.Email}",
                CreatedAt = now
            };
            _context.Transactions.AddRange(outgoingTransaction,incomingTransaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return (true, "Transfer completed successfully.");
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}