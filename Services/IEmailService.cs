namespace MasterCard.Services;
public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string subject,
        string plainText,
        string htmlContent,
        CancellationToken cancellationToken = default);
}