using System;
using NotificationSystem.DataAccess.Models;
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

        public List<User> GetAllUsers()
        {
            var entities = _repository.GetAll();
            return entities.Select(MapToBusinessModel).ToList();
        }

        public User? GetUserById(int id)
        {
            var entity = _repository.GetById(id);
            return entity != null ? MapToBusinessModel(entity) : null;
        }

        public User CreateUser(string name, string email, string phoneNumber)
        {
            // check if email is already exists
            if (_repository.ExistsByEmail(email))
            {
                throw new InvalidOperationException($"user with this {email} already exists.");
            }
            //ceate business model (it will trigger the validation)
            var user = new User
            {
                Name = name,
                Email = email,
                PhoneNumber = phoneNumber,
                IsActive = true,
                ReceiveEmailNotifications = true,
                ReceiveSmsNotifications = true,
                CreatedAt = DateTime.Now
            };

            // covert to storage entity
            var entity = new UserEntity
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNum = user.PhoneNumber,
                IsActive = user.IsActive,
                ReceiveEmailNotification = user.ReceiveEmailNotification,
                ReceiveSmsNotification = user.ReceiveSmsNotification,
                CreatedAt = user.CreatedAt
        };

        _repository.Add(entity);
        user.Id = entity.Id;

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