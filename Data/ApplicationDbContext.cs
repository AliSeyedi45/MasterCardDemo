using MasterCard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace MasterCard.Data;
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<PasswordResetCode> PasswordResetCodes
    {
        get;
        set;
    }
    public DbSet<Transaction> Transactions
    {
        get;
        set;
    }
    public DbSet<ChargeRequest> ChargeRequests
    {
        get;
        set;
    }
}