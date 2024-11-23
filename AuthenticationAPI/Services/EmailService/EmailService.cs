using AuthenticationAPI.Models;
using AuthenticationAPI.Models.ConfigModels;
using MimeKit;
using MailKit.Net.Smtp;

namespace AuthenticationAPI.Services.EmailService
{
    public class EmailService : IEmailService
    {
        public readonly EmailCred _emailCred;

        public EmailService(EmailCred emailCred)
        {
            _emailCred = emailCred;
        }

        public async Task<bool> SendEmail(EmailMessage message)
        {
            //Convert to MimeMessage
            MimeMessage messageToSend = new MimeMessage();
            messageToSend.From.Add(new MailboxAddress("Trafficy", message.From));
            messageToSend.To.Add(new MailboxAddress("Name", message.To));
            messageToSend.Subject = message.Subject;
            messageToSend.Body = new TextPart("html")
            {
                Text = message.Body
            };
            try
            {
                using (var client = new SmtpClient())
                {
                    // Connect to the SMTP server
                    client.Connect("mail.privateemail.com", 465, true);

                    // Authenticate
                    client.Authenticate(_emailCred.Username, _emailCred.Password);

                    // Send the email
                    client.Send(messageToSend);
                    client.Disconnect(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                return false;
            }
        }
    }
}
