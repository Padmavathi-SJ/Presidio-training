using UsersXL.Models;

namespace UsersXL.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}