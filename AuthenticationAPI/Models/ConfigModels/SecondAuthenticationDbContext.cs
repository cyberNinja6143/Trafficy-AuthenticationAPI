using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Models.ConfigModels
{
    public class SecondAuthenticationDbContext : DbContext
    {
        public SecondAuthenticationDbContext(DbContextOptions<SecondAuthenticationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
