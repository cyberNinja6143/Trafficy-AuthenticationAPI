using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Models.ConfigModels
{
    public class AuthenticationDbContext : DbContext
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
