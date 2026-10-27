using MailKit.Net.Smtp;
using MailKit.Security;
using MasterCard.Settings;
using Microsoft.Extensions.Options;
using MimeKit;
namespace MasterCard.Services;
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
    public async Task SendAsync(
        string toEmail,
        string subject,
        string plainText,
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.GmailAddress) ||
            string.IsNullOrWhiteSpace(_settings.GmailAppPassword))
        {
            throw new InvalidOperationException(
                "Gmail email settings are not configured.");
        }
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("MasterCard", _settings.GmailAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        var body = new BodyBuilder
        {
            TextBody = plainText,
            HtmlBody = htmlContent
        };
        message.Body = body.ToMessageBody();
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com",587,SecureSocketOptions.StartTls,cancellationToken);
        try
        {
            await client.AuthenticateAsync(_settings.GmailAddress,_settings.GmailAppPassword,cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}