using Contracts;
using Entities.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Mobile_Backend.Helper
{
    public class EmailSender : IEmailSender { 

        public readonly EmailSettings _emailSettings;
        private ILoggerManager _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings) {
            _emailSettings = emailSettings.Value;
            _logger = new LoggerService.LoggerManager();
        }
    
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));
                mimeMessage.To.Add(new MailboxAddress(email));
                mimeMessage.Subject = subject;

                mimeMessage.Body = new TextPart("html")
                {
                    Text = message
                };

                using (var client = new SmtpClient()) {

                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_emailSettings.MailServer);
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

                    await client.SendAsync(mimeMessage);

                    await client.DisconnectAsync(true);

                    _logger.LogInfo("Email sent successfully: " + email);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Problem occured during sending an email: " + e.Message); 
            }
        }
    }
}
