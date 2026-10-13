using MasterCard.Data;
using MasterCard.DTOs;
using MasterCard.Models;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Services;
public class ChargeService
{
    private readonly ApplicationDbContext _context;
    public ChargeService(
        ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<(bool Success, string Message)>
        CreateChargeRequestAsync(string userId,string code,decimal amount)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit))
        {
            return (false,"Charge code must be exactly 6 digits.");
        }
        if (amount <= 0)
        {
            return (false,"Amount must be greater than zero.");
        }
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return (false,"User not found.");
        }
        if (!user.IsActive)
        {
            return (false,"This account is inactive.");
        }
        var chargeRequest = new ChargeRequest
        {
            UserId = userId,
            Code = code,
            Amount = amount,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChargeRequests.Add(chargeRequest);
        await _context.SaveChangesAsync();
        return (true,"Charge request created successfully.");
    }
    public async Task<List<ChargeRequestResponse>>
        GetPendingChargeRequestsAsync()
    {
        return await _context.ChargeRequests
            .Where(x => !x.IsApproved)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ChargeRequestResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                UserEmail = x.User!.Email,
                Code = x.Code,
                Amount = x.Amount,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }
    public async Task<(bool Success, string Message)>
        ApproveChargeAsync(int chargeRequestId)
    {
        var chargeRequest = await _context.ChargeRequests.FirstOrDefaultAsync(x => x.Id == chargeRequestId);
        if (chargeRequest == null)
        {
            return (false,"Charge request not found.");
        }
        if (chargeRequest.IsApproved)
        {
            return (false,"This charge request has already been approved.");
        }
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == chargeRequest.UserId);
        if (user == null)
        {
            return (false,"User not found.");
        }
        if (!user.IsActive)
        {
            return (false,"This account is inactive.");
        }
        user.Balance += chargeRequest.Amount;
        chargeRequest.IsApproved = true;
        chargeRequest.ApprovedAt = DateTime.UtcNow;
        var transaction = new Transaction
        {
            UserId = user.Id,
            Type = TransactionType.Charge,
            Amount = chargeRequest.Amount,
            Description = "Account charge",
            CreatedAt = DateTime.UtcNow
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return (true,"Charge request approved successfully.");
    }
}