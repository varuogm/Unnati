using Microsoft.Extensions.Options;
using Unnati.Models;
using MailKit.Security;
using MimeKit;
using Unnati.Service;
using SendGrid;

namespace Unnati.Container
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;

        private readonly ILogger<EmailService> _logger;
        public EmailService(IOptions<EmailSettings> options , ILogger<EmailService> logger)
        {
            this.emailSettings = options.Value;
            this._logger = logger;
        }
        public async Task SendEmail(Mailrequest mailrequest)
        {
            var email = new MimeMessage();
            //email.Sender = MailboxAddress.Parse(emailSettings.Username);
            email.Sender = MailboxAddress.Parse("gouravmajee1999@gmail.com");

            email.To.Add(MailboxAddress.Parse(mailrequest.Email));
            email.Subject = mailrequest.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailrequest.Emailbody;
            email.Body = builder.ToMessageBody();

            try
            {
                using var smptp = new MailKit.Net.Smtp.SmtpClient();
                smptp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);

                var isSmtpConnected =  smptp.IsConnected;

                smptp.Authenticate(emailSettings.Username, emailSettings.Password);

                var isAuthenticated = smptp.IsAuthenticated.ToString();

                await smptp.SendAsync(email);
                smptp.MessageSent += (sender, args) =>
                {
                    Console.WriteLine("Email has been sent successfully");
                };
                smptp.Disconnect(true);
            }
            catch (Exception ex)
            {
                // Log the exception here
                _logger.LogError("Error while sending email",ex);
                throw;
            }
        }
    }
}
