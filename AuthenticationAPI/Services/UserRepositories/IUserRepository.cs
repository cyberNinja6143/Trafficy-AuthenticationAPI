using AuthenticationAPI.Models;

namespace AuthenticationAPI.Services.UserRepositories
{
    public interface IUserRepository
    {
        Task<User> GetByUUID(Guid UUID);
        Task<User> GetByEmail(string email);
        Task<User> Create(User user);
        Task<bool> DeleteById(Guid id);
        Task<string> GetAllUsersAsString();
        Task<bool> Update(User user);
    }
}
