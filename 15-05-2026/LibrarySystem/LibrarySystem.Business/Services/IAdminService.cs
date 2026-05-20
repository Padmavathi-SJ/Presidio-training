
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IAdminService
    {
        Task<bool> LoginAsync(string phoneNum, string password);
    }
}

