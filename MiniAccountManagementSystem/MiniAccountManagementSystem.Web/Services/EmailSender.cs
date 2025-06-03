using Microsoft.AspNetCore.Identity.UI.Services;
namespace MiniAccountManagementSystem.Web.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For development/testing - just log the email
            Console.WriteLine($"Email to: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");
            // Return completed task since we're not actually sending email
            return Task.CompletedTask;
        }
    }
}