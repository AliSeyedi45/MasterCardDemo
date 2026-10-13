using MasterCard.DTOs;
using MasterCard.Models;
using MasterCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Controllers;
[ApiController]
[Route("Api/Admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ChargeService _chargeService;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminController(ChargeService chargeService,
        UserManager<ApplicationUser> userManager)
    {
        _chargeService = chargeService;
        _userManager = userManager;
    }
    [HttpGet("Users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users.OrderBy(u => u.CreatedAt).ToListAsync();         
        var result = new List<object>();
        foreach (var u in users)
        {
            result.Add(new
            {
                id = u.Id,
                email = u.Email,
                phoneNumber = u.PhoneNumber,
                isActive = u.IsActive,
                twoFactorEnabled = u.TwoFactorEnabled,
                balance = u.Balance,
                createdAt = u.CreatedAt,
                roles = await _userManager.GetRolesAsync(u)
            });
        }
        return Ok(result);
    }
    [HttpGet("Charges")]
    public async Task<IActionResult> GetPendingCharges()
    {
        var requests = await _chargeService.GetPendingChargeRequestsAsync();
        return Ok(requests);
    }
    [HttpPost("Charges/Approve")]
    public async Task<IActionResult> ApproveCharge(
        ApproveChargeRequest request)
    {
        var result = await _chargeService.ApproveChargeAsync(request.Id);
        if (!result.Success)
            return BadRequest(new
            {
                message = result.Message
            });
        return Ok(new
        {
            message = result.Message
        });
    }
}