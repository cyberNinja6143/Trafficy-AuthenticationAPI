using AuthenticationAPI.Models;
using AuthenticationAPI.Models.ConfigModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthenticationAPI.Services.UserRepositories
{
    public class SqliteUserRepository : IUserRepository
    {
        private readonly AuthenticationDbContext _context;

        public SqliteUserRepository(AuthenticationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteById(Guid id)
        {
            // Find the user by username
            var user = await _context.Users.FindAsync(id);

            // If the user doesn't exist, return false
            if (user == null)
            {
                return false;
            }

            // Remove the user and save changes
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(User user)
        {
            // Find the existing user in the database
            var existingUser = await _context.Users.FindAsync(user.UUID); // Assuming 'Id' is the Guid property

            // If the user doesn't exist, return false
            if (existingUser == null)
            {
                return false;
            }

            // Update properties
            existingUser.Username = user.Username; // Update properties as needed
            existingUser.Email = user.Email; // Example property, add others as necessary

            // Mark the entity as modified (optional, as EF tracks changes automatically)
            _context.Users.Update(existingUser);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User> GetByUUID(Guid UUID)
        {
            return await _context.Users.FindAsync(UUID);
        }

        public async Task<string> GetAllUsersAsString()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null || users.Count == 0)
            {
                return "No users found.";
            }

            // Build a string representation of all users
            var usersString = string.Join(Environment.NewLine, users.Select(user => $"Username: {user.Username}, UUID: {user.UUID}, Email: {user.Email}, Confirmed: {user.EmailConfirmed}"));

            return usersString;
        }
    }
}
