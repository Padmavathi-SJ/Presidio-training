using System;
using System.Collections.Generic;
using NotificationSystem.DataAccess.Entities;

namespace NotificationSystem.DataAccess.Repositories
{
    public interface IUserRepository
    {
       Task<List<UserEntity>> GetAllAsync();
        Task<UserEntity?> GetByIdAsync(int id);
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity> AddAsync(UserEntity user);
       Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
       Task<int> GetNextIdAsync();
    }
}