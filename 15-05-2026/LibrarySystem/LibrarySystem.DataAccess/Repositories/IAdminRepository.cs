using System;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IAdminRepository
    {
        Task<bool>  ExistsByPhoneNumAsync(string phoneNum);
        Task<Admin?> GetByPhoneNumAsync(string phoneNum);
        Task<int> GetNextIdAsync();
    }
}