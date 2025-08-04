using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagement.Infrastructure.Configuration;

namespace TaskManagement.Infrastructure.Services;

public interface IEmailService
{
    Task SendTaskReminderAsync(string toEmail, string toName, string taskTitle, DateTime dueDate);
    Task SendTaskAssignmentAsync(string toEmail, string toName, string taskTitle, string assignedBy);
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendTaskReminderAsync(string toEmail, string toName, string taskTitle, DateTime dueDate)
    {
        var subject = $"Task Reminder: {taskTitle}";
        var body = $@"
            <html>
            <body>
                <h2>Task Reminder</h2>
                <p>Hello {toName},</p>
                <p>This is a reminder that your task <strong>'{taskTitle}'</strong> is due on <strong>{dueDate:yyyy-MM-dd HH:mm}</strong>.</p>
                <p>Please make sure to complete it on time.</p>
                <br/>
                <p>Best regards,<br/>Task Management System</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    public async Task SendTaskAssignmentAsync(string toEmail, string toName, string taskTitle, string assignedBy)
    {
        var subject = $"New Task Assigned: {taskTitle}";
        var body = $@"
            <html>
            <body>
                <h2>New Task Assignment</h2>
                <p>Hello {toName},</p>
                <p>You have been assigned a new task: <strong>'{taskTitle}'</strong> by {assignedBy}.</p>
                <p>Please check your task board for more details.</p>
                <br/>
                <p>Best regards,<br/>Task Management System</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, toName, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, 
                _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            if (!string.IsNullOrEmpty(_smtpSettings.Username))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject '{Subject}'", toEmail, subject);
            throw;
        }
    }
}
