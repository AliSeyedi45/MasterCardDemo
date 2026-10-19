using System.Security.Claims;
using MasterCard.DTOs;
using MasterCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MasterCard.Controllers;
[ApiController]
[Route("api/transfers")]
[Authorize]
public class TransferController : ControllerBase
{
    private readonly TransferService _transferService;
    public TransferController(TransferService transferService)
    {
        _transferService = transferService;
    }
    [HttpPost]
    public async Task<IActionResult> Transfer(
        TransferRequest request)
    {
        var senderUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(senderUserId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        var result = await _transferService.TransferAsync(senderUserId,request);
        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }
        return Ok(new
        {
            message = result.Message
        });
    }
}