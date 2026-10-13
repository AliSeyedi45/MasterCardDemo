using MasterCard.DTOs;
using MasterCard.Models;
using MasterCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace MasterCard.Controllers;
[ApiController]
[Route("Api/Auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }
        return Ok(new
        {
            message = result.Message,
            userId = result.UserId
        });
    }
    [HttpPost("Login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email,request.Password);
        if (!result.Success)
        {
            return Unauthorized(new
            {
                message = result.Message
            });
        }
        return Ok(new
        {
            message = result.Message,requiresTwoFactor = result.RequiresTwoFactor
        });
    }
    [HttpPost("Login/2FA")]
    public async Task<IActionResult> VerifyTwoFactorLogin(
        TwoFactorLoginRequest request)
    {
        var result = await _authService.VerifyTwoFactorLoginAsync(request.Code);
        if (!result.Success)
        {
            return Unauthorized(new
            {
                message = result.Message
            });
        }
        return Ok(new
        {
            message = result.Message
        });
    }
    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new
        {
            message = "Logout Successful"
        });
    }
    [Authorize]
    [HttpGet("Me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName,
            phoneNumber = user.PhoneNumber,
            isActive = user.IsActive,
            twoFactorEnabled = user.TwoFactorEnabled,
            balance = user.Balance,      
          //  roles = roles,               
            createdAt = user.CreatedAt
        });
    }
    [Authorize]
    [HttpPost("Change-Password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var user = await _authService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        var result = await _authService.ChangePasswordAsync(user,request.CurrentPassword,request.NewPassword);
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
    [HttpPost("Forgot-Password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);
        return Ok(new
        {
            message = result.Message,resetCode = result.Code
        });
    }
    [HttpPost("Verify_Reset-Code")]
    public async Task<IActionResult> VerifyResetCode(
        VerifyResetCodeRequest request)
    {
        var result = await _authService.VerifyResetCodeAsync(request.Email,request.Code);
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
    [HttpPost("Reset-Password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request.Email,request.ResetToken,request.NewPassword);
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
    [Authorize]
    [HttpGet("2FA/Setup")]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var user = await _authService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        var result = await _authService.GetTwoFactorSetupAsync(user);
        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }
        return Ok(new
        {
            message = result.Message,sharedKey = result.SharedKey
        });
    }
    [Authorize]
    [HttpPost("2FA/Enable")]
    public async Task<IActionResult> EnableTwoFactor(
        EnableTwoFactorRequest request)
    {
        var user = await _authService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        var result = await _authService.EnableTwoFactorAsync(user,request.Code);
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
    [Authorize]
    [HttpPost("2FA/Disable")]
    public async Task<IActionResult> DisableTwoFactor(
        DisableTwoFactorRequest request)
    {
        var user = await _authService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }
        var result = await _authService.DisableTwoFactorAsync(user,request.Code);
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