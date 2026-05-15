using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using UMS.Application.Common.Interfaces;
using UMS.Infrastructure.Settings;

namespace UMS.Infrastructure.Email
{
    public sealed class MailKitEmailService(IOptions<EmailSettings> options) : IEmailService
    {
        private readonly EmailSettings _settings = options.Value;

        public async Task SendApplicationReceivedAsync(
            string toEmail, string fullName, CancellationToken ct = default)
        {
            await SendAsync(
                toEmail: toEmail,
                toName: fullName,
                subject: "Your Usher Application Has Been Received",
                htmlBody: EmailTemplates.ApplicationReceived(fullName),
                ct: ct);
        }

        public async Task SendPasswordSetupAsync(
            string toEmail, string fullName, string token, CancellationToken ct = default)
        {
            var setupUrl = $"{_settings.FrontendUrl}/set-password?token={token}";

            await SendAsync(
                toEmail: toEmail,
                toName: fullName,
                subject: "Your Application is Approved — Set Your Password",
                htmlBody: EmailTemplates.PasswordSetup(fullName, setupUrl),
                ct: ct);
        }

        private async Task SendAsync(
            string toEmail, string toName,
            string subject, string htmlBody,
            CancellationToken ct)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                ct);

            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(quit: true, ct);
        }
    }
}
