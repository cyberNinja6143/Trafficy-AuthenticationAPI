using AuthenticationAPI.Models;

namespace AuthenticationAPI.Services.EmailService
{
    public interface IEmailService
    {
        Task<Boolean> SendEmail(EmailMessage message);
    }
}
