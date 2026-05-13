using System;
using System.Collections.Generic;
using WordGuessingGame.Models;

namespace WordGuessingGame.Repositories
{
    public interface IUserRepository
    {
        List<UserEntity> GetAll();
        UserEntity? GetById(int id);
        UserEntity? GetByName(string name);
        void Add(UserEntity user);
        bool ExistsByName(string name);
        UserEntity? Authenticate(string name, string password);
    }
}