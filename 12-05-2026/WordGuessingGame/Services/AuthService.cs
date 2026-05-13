using System;
using WordGuessingGame.Database;
using WordGuessingGame.Models;
using WordGuessingGame.Repositories;

namespace WordGuessingGame.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private UserEntity? _currentUser;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public UserEntity? CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;
        public string CurrentUserName => _currentUser?.Name ?? "Guest";

        public (bool success, string message) Register(string name, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name))
            
                return (false, "name should not be empty ");

            if (_userRepository.ExistsByName(name))
            
                return (false, "user already exists, please choose another name.");
            
            if(name.Length < 3)
            
                return (false, "name should have length atleast 3 characters.");
            
            if (name.Length > 20)
                return (false, "Username cannot exceed 20 characters");

            if(string.IsNullOrWhiteSpace(password))
                return (false, "password cannot be empty.");
            
            if(password != confirmPassword)
            
                return (false, "password do not match!");
            
            var user = new UserEntity
            {
                Name = name.Trim(),
                Password = password,
                CreatedAt = DateTime.Now
            };

            _userRepository.Add(user);
            return (true, $"Registration Successfull! Welcome {name}!");
        }

        public (bool success,string message) Login(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "name should not be empty ");
            
            if(string.IsNullOrWhiteSpace(password))
                return (false, "password cannot be empty.");

            var user = _userRepository.Authenticate(name, password);

            if(user == null)
                return (false, "Invalid username and password");
            
            _currentUser = user;
            return (true, $"Welcome back, {user.Name}");
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}