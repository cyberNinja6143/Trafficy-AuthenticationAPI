using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Models.Requests
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string regestrationToken { get; set; }
    }
}
