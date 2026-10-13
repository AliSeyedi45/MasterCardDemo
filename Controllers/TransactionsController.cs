using MasterCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace MasterCard.Controllers;
[ApiController]
[Route("Api/Transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;
    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    [HttpGet]
    public async Task<IActionResult> GetMyTransactions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var transactions = await _transactionService.GetMyTransactionsAsync(userId);
        return Ok(transactions);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTransaction(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var transaction = await _transactionService.GetTransactionByIdAsync(id,userId);
        if (transaction == null)
        {
            return NotFound(new
            {
                message = "Transaction not found."
            });
        }
        return Ok(transaction);
    }
}