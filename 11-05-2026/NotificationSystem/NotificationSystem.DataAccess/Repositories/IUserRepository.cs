using System;
using System.Collections.Generic;
using NotificationSystem.DataAccess.Models;

namespace NotificationSystem.DataAccess.Repositories
{
    public interface IUserRepository
    {
        List<UserEntity> GetAll();
        UserEntity? GetById(int id);
        UserEntity? GetByEmail(string email);
        void Add(UserEntity user);
        bool Exists(int id);
        bool ExistsByEmail(string email);
       int GetNextId();
    }
}