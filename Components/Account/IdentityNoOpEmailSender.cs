using BlazorSocial.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace BlazorSocial.Components.Account {
    // Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser> {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public static Task SendEmailAsync(string email, string subject, string htmlMessage) {
            var smtpClient = new SmtpClient("smtp.gmail.com") {
                Port = 587,
                Credentials = new NetworkCredential("scottyboy552@gmail.com", "Tm08K006o7Y6"),
                EnableSsl = true,
            };

            smtpClient.Send("scottyboy552@gmail.com", email, subject, htmlMessage);

            return Task.CompletedTask;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) {
            return emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
