using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using PeiuPlatform.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email_sender, string subject, string message, params string[] address);

        Task SendEmailAsync(string email_sender, string subject, string message, RegisterFileRepositaryEF attachFile, params string[] address);
        
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHostingEnvironment _env;

        public EmailSender(
            IOptions<EmailSettings> emailSettings,
            IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendEmailAsync(string email_sender, string subject, string message, RegisterFileRepositaryEF attachFile, params string[] emails)
        {
            try
            {
                if (emails.Length == 0)
                    return;
                //await Task.CompletedTask;
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(email_sender, _emailSettings.Sender));
                IEnumerable<MailboxAddress> address = emails.Select(x => new MailboxAddress(x));
                mimeMessage.To.AddRange(address);

                mimeMessage.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                if (attachFile != null )
                {
                    bodyBuilder.Attachments.Add(attachFile.FileName, attachFile.Contents);
                }

                bodyBuilder.HtmlBody = message;
                mimeMessage.Body = bodyBuilder.ToMessageBody();


                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, true);

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

                    await client.SendAsync(mimeMessage);
                    Console.WriteLine($"이메일 전송: {string.Join(',', emails)}");

                    await client.DisconnectAsync(true);
                }

            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task SendEmailAsync(string email_sender, string subject, string message, params string[] emails)
        {
            try
            {

                //await Task.CompletedTask;
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(email_sender, _emailSettings.Sender));
                IEnumerable<MailboxAddress> address = emails.Select(x => new MailboxAddress(x));
                mimeMessage.To.AddRange(address);

                mimeMessage.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                

                bodyBuilder.HtmlBody = message;
                mimeMessage.Body = bodyBuilder.ToMessageBody();


                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, true);

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

                    await client.SendAsync(mimeMessage);
                    Console.WriteLine($"이메일 전송: {string.Join(',', emails)}");

                    await client.DisconnectAsync(true);
                }

            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }
        }
    }

    public class EmailSettings
    {
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
    }
}
