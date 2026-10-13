using Microsoft.AspNetCore.Identity;
namespace MasterCard.Models;
public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public decimal Balance { get; set; } = 0;
}