using AuthenticationAPI.Models;
using AuthenticationAPI.Models.ConfigModels;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Services.HouseKeeping
{
    public class Cleaner
    {
        private readonly IDbContextFactory<SecondAuthenticationDbContext> _dbContextFactory;

        public Cleaner(IDbContextFactory<SecondAuthenticationDbContext> factory)
        {
            _dbContextFactory = factory;
        }

        public async Task CleanIfEmailDoesNotVerify(Guid guid)
        {
            double minutes = 6;
            int convertedToMiliSeconds = (int)(minutes * 60000);
            Console.WriteLine($"New User added If user does not verify their email in {minutes} minutes the user will be removed");
            await Task.Delay(convertedToMiliSeconds);

            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = await context.Users.FindAsync(guid);
                if (user == null)
                {
                    return;
                }
                if (user.EmailConfirmed)
                {
                    return;
                }
                Console.WriteLine("The email was not confirmed in time");
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                Console.WriteLine("User has been deleted");
            }
        }
    }
}
