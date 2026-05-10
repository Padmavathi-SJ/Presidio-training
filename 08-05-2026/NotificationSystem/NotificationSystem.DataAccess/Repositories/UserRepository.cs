using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using NotificationSystem.DataAccess.Models;

namespace NotificationSystem.DataAccess.Repositories
{
    public class UserRepository : IUserRepository{

    private readonly String file_path = "users.json";
    private List<UserEntity> _users = new List<UserEntity>();

    public UserRepository()
    {
        LoadData();
    }

    private void LoadData()
    {
        if (File.Exists(file_path))
        {
            var json = File.ReadAllText(file_path);
            _users = JsonSerializer.Deserialize<List<UserEntity>>(json) ?? new List<UserEntity>();
        }
        else
        {
            _users = new List<UserEntity>();
            SaveData();
        }
    }

    private void SaveData()
    {
        var json = JsonSerializer.Serialize(_users);
        File.WriteAllText(file_path, json);
    }

    public List<UserEntity> GetAll()
    {
        return _users.ToList();

    }

    public UserEntity? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }
    public UserEntity? GetByEmail(string email)
    {
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(UserEntity user)
    {
        user.Id = GetNextId();
        user.CreatedAt = DateTime.Now;
        _users.Add(user);
        SaveData();
    }
    public bool Exists(int id)
    {
        return _users.Any(u => u.Id == id);
    }

    public bool ExistsByEmail(string email)
    {
        return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public int GetNextId()
    {
        return _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
    }
}
}
