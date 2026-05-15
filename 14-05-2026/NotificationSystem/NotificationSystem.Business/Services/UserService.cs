using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationSystem.DataAccess.Entities;
using NotificationSystem.DataAccess.Repositories;
using NotificationSystem.Business.Models;

namespace NotificationSystem.Business.Services
{
    public class UserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToBusinessModel).ToList();
        }

        //  sync version for backward compatibility
        public List<User> GetAllUsers()
        {
            return GetAllUsersAsync().GetAwaiter().GetResult();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? MapToBusinessModel(entity) : null;
        }

        public User? GetUserById(int id)
        {
            return GetUserByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task<User> CreateUserAsync(string name, string email, string phoneNumber)
        {
            if (await _repository.ExistsByEmailAsync(email))
            {
                throw new InvalidOperationException($"User with this {email} already exists.");
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PhoneNumber = phoneNumber,
                IsActive = true,
                ReceiveEmailNotifications = true,
                ReceiveSmsNotifications = true,
                CreatedAt = DateTime.UtcNow
            };

            var entity = new UserEntity
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNum = user.PhoneNumber,
                IsActive = user.IsActive,
                ReceiveEmailNotification = user.ReceiveEmailNotifications,
                ReceiveSmsNotification = user.ReceiveSmsNotifications,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(entity);
            user.Id = entity.Id;

            return user;
        }

public User CreateUser(string name, string email, string phoneNumber)
{
    Console.WriteLine($"[DEBUG] CreateUser called for: {name}, {email}");
    

    
    var user = new User
    {
        Name = name,
        Email = email,
        PhoneNumber = phoneNumber,
        IsActive = true,
        ReceiveEmailNotifications = true,
        ReceiveSmsNotifications = true,
        CreatedAt = DateTime.UtcNow
    };

    var entity = new UserEntity
    {
        Name = user.Name,
        Email = user.Email,
        PhoneNum = user.PhoneNumber,
        IsActive = user.IsActive,
        ReceiveEmailNotification = user.ReceiveEmailNotifications,
        ReceiveSmsNotification = user.ReceiveSmsNotifications,
        CreatedAt = DateTime.UtcNow
    };

    Console.WriteLine($"[DEBUG] Calling repository AddAsync...");
    _repository.AddAsync(entity);  // Make sure this is async? Wait!
    Console.WriteLine($"[DEBUG] Repository AddAsync completed");
    
    user.Id = entity.Id;
    Console.WriteLine($"[DEBUG] User created with ID: {user.Id}");

    return user;
}
        private User MapToBusinessModel(UserEntity entity)
        {
            return new User
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNum,
                IsActive = entity.IsActive,
                ReceiveEmailNotifications = entity.ReceiveEmailNotification,
                ReceiveSmsNotifications = entity.ReceiveSmsNotification,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}