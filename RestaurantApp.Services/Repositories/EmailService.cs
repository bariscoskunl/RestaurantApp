using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp; 
using RestaurantApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace RestaurantApp.Services.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {           
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"],
                emailSettings["Email"]
                ));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(
                emailSettings["SmtpServer"],
                int.Parse(emailSettings["Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls
                );
            await client.AuthenticateAsync(
                emailSettings["Email"],
                emailSettings["Password"]
                );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
