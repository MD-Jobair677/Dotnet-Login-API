using MailKit.Net.Smtp;
using MimeKit;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(
        string to,
        string subject,
        string body)
    {
        var email = new MimeMessage();

        email.From.Add(
            MailboxAddress.Parse("mdjobairhossain618@gmail.com"));

        email.To.Add(
            MailboxAddress.Parse(to));

        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            "smtp.gmail.com",
            587,
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            "mdjobairhossain618@gmail.com",
            "dwkrgdeiakfhbujb" // app password
        );

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}