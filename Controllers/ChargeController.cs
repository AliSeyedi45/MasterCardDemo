using MasterCard.DTOs;
using MasterCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace MasterCard.Controllers;
[ApiController]
[Route("Api/Charge")]
[Authorize]
public class ChargeController : ControllerBase
{
    private readonly ChargeService _chargeService;
    public ChargeController(
        ChargeService chargeService)
    {
        _chargeService = chargeService;
    }
    [HttpPost]
    public async Task<IActionResult> CreateChargeRequest(CreateChargeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var result = await _chargeService.CreateChargeRequestAsync(userId,request.Code,request.Amount);
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