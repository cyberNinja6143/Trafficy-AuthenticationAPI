using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Models
{
    public class User
    {
        [Key]
        public Guid UUID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
