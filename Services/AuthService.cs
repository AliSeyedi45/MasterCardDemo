using System.Security.Claims;
using System.Security.Cryptography;
using MasterCard.Data;
using MasterCard.DTOs;
using MasterCard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Services;
public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    public async Task<(bool Success, string Message, string? UserId)>
        RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return (false,"This Email Already Exists",null);
        }
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            IsActive = true,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user,request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return (false,errors,null);
        }
        return (true,"User Registered Successfully",user.Id);
    }
    public async Task<(bool Success, string Message, bool RequiresTwoFactor)>
        LoginAsync(
            string email,
            string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false,"Invalid email or password.",false);
        }
        if (!user.IsActive)
        {
            return (false,"This account is inactive.",false);
        }
        await _userManager.UpdateSecurityStampAsync(user);
        var result = await _signInManager.PasswordSignInAsync(
                user,
                password,
                isPersistent: false,
                lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return (false,"Your account is temporarily locked.",false);
        }
        if (result.IsNotAllowed)
        {
            return (false,"Login is not allowed.",false);
        }
        if (result.RequiresTwoFactor)
        {
            return (true,"Two-factor authentication code is required.",true);
        }
        if (!result.Succeeded)
        {
            return (false,"Invalid email or password.",false);
        }
        return (true,"Login Successful",false);
    }
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
    public async Task<ApplicationUser?>
        GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }
    public async Task<(bool Success, string Message)>
        ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user,currentPassword,newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return (false,errors);
        }
        return (true,"Password Changed Successfully");
    }
    public async Task<(bool Success, string Message, string? Code)>
        ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return (true,"If this email exists, a password reset code has been created.",null);
        }
        var oldCodes = await _context.PasswordResetCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .ToListAsync();
        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
        }
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var now = DateTime.UtcNow;
        var resetCode = new PasswordResetCode
        {
            UserId = user.Id,
            Code = code,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(5),
            IsUsed = false
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();
        return (true,"Password reset code created successfully.",code);
    }
    public async Task<(bool Success, string Message)>
        VerifyResetCodeAsync(
            string email,
            string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return (false,"Invalid email or reset code.");
        }
        var resetCode = await _context.PasswordResetCodes
                .Where(x => x.UserId == user.Id && x.Code == code && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        if (resetCode == null)
        {
            return (false,"Invalid reset code.");
        }
        if (resetCode.ExpiresAt <= DateTime.UtcNow)
        {
            return (false,"Reset code has expired.");
        }
        return (true,"Reset code is valid.");
    }
    public async Task<(bool Success, string Message)>
        ResetPasswordAsync(
            string email,
            string resetToken,
            string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return (false,"Invalid password reset request.");
        }

        var resetCode = await _context.PasswordResetCodes
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Code == resetToken &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        if (resetCode == null)
        {
            return (false,"Invalid reset code.");
        }
        if (resetCode.ExpiresAt <= DateTime.UtcNow)
        {
            return (false,"Reset code has expired.");
        }
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user,newPassword);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ",updateResult.Errors.Select(e => e.Description));
            return (false,errors);
        }
        resetCode.IsUsed = true;
        await _context.SaveChangesAsync();
        return (true,"Password Reset Successfully");
    }
    public async Task<(bool Success, string Message, string? SharedKey)>
        GetTwoFactorSetupAsync(
            ApplicationUser user)
    {
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);

            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }
        if (string.IsNullOrEmpty(key))
        {
            return (false,"Could not generate authenticator key.",null);
        }
        return (true,"Authenticator key generated successfully.",key);
    }
    public async Task<(bool Success, string Message)>
        EnableTwoFactorAsync(
            ApplicationUser user,
            string code)
    {
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user,_userManager
                    .Options
                    .Tokens
                    .AuthenticatorTokenProvider,
                code);
        if (!isValid)
        {
            return (false,"Invalid authenticator code.");
        }
        var result = await _userManager.SetTwoFactorEnabledAsync(user,true);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return (false,errors);
        }
        return (true,"2F authentication enabled successfully.");
    }
    public async Task<(bool Success, string Message)>
        DisableTwoFactorAsync(
            ApplicationUser user,
            string code)
    {
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user,_userManager.Options.Tokens.AuthenticatorTokenProvider,code);
        if (!isValid)
        {
            return (false,"Invalid authenticator code.");
        }
        var result = await _userManager.SetTwoFactorEnabledAsync(user,false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return (false,errors);
        }
        return (true,"2F authentication disabled successfully.");
    }
    public async Task<(bool Success, string Message)>
    VerifyTwoFactorLoginAsync(string code)
    {
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code,isPersistent: false,rememberClient: false);
        if (result.Succeeded)
        {
            return (true,"Login Successful");
        }
        if (result.IsLockedOut)
        {
            return (false,"Your account is temporarily locked.");
        }
        if (result.IsNotAllowed)
        {
            return (false,"Login is not allowed.");
        }
        return (false,"Invalid authenticator code.");
    }
    public Task<IList<string>> GetRolesAsync(ApplicationUser user) => _userManager.GetRolesAsync(user);
}