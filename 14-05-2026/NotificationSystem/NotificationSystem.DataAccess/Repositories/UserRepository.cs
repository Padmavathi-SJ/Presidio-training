using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using Npgsql;
using NotificationSystem.DataAccess.Entities;
using NotificationSystem.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace NotificationSystem.DataAccess.Repositories
{
    public class UserRepository : IUserRepository{

    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

    public async Task<List<UserEntity>> GetAllAsync()
        {
            return await _context.Users
                 .OrderBy(u => u.Id)
                 .ToListAsync();
        }
    
    public async Task<UserEntity?> GetByIdAsync(int id)
        {
            return await _context.Users
                  .FirstOrDefaultAsync(u => u.Id == id);
        }
    public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == email);
        }
    
    public async Task<UserEntity> AddAsync(UserEntity user)
        {
            Console.WriteLine($"[DEBUG] Adding user: {user.Name}, {user.Email}");
            _context.Users.Add(user);
            var result = await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] SaveChangesAsync returned: {result} rows affected");
    Console.WriteLine($"[DEBUG] New user ID: {user.Id}");
            return user;
        }

    public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }
    
    public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    
    public async Task<int> GetNextIdAsync()
        {
            var maxId = await _context.Users.MaxAsync(u => (int?)u.Id) ?? 0; 
            return maxId + 1;
        }

/*
    public List<UserEntity> GetAll()
    {
        var users = new List<UserEntity>();

       using var conn = _dbConnection.GetConnection();
        string query = "select id, name, email, phone_num, isactive, receiveemailnotification, receivesmsnotification, createdAt from users order by id";
        var cmd = new NpgsqlCommand(query, conn);

        var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PhoneNum = reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    ReceiveEmailNotification = reader.GetBoolean(5),
                    ReceiveSmsNotification = reader.GetBoolean(6),
                    CreatedAt = reader.GetDateTime(7)
                });
                
            }
             return users;
    }

    public UserEntity? GetById(int id)
    {
       using var conn = _dbConnection.GetConnection();
       string query = "SELECT id, name, email, phone_num, isactive, receiveemailnotification, receivesmsnotification, createdat FROM users WHERE id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PhoneNum = reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    ReceiveEmailNotification = reader.GetBoolean(5),
                    ReceiveSmsNotification = reader.GetBoolean(6),
                    CreatedAt = reader.GetDateTime(7)
                };
            }
            return null;
    }


    public UserEntity? GetByEmail(string email)
    {
     using var conn = _dbConnection.GetConnection();
        string query = "SELECT id, name, email, phone_num, isactive, receiveemailnotification, receivesmsnotification, createdat FROM users WHERE email = @email";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            using var reader = cmd.ExecuteReader();


        while (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PhoneNum = reader.GetString(3),
                    IsActive = reader.GetBoolean(4),
                    ReceiveEmailNotification = reader.GetBoolean(5),
                    ReceiveSmsNotification = reader.GetBoolean(6),
                    CreatedAt = reader.GetDateTime(7)
                };
            }
        return null;

    }

    public void Add(UserEntity user)
    {
       using var conn = _dbConnection.GetConnection();
        string query = @"
        INSERT INTO users (name, email, phone_num, isactive, receiveemailnotification, receivesmsnotification, createdat) 
        VALUES (@name, @email, @phone_num, @isactive, @receiveemailnotification, @receivesmsnotification, @createdat)
        returning id";

        var cmd = new NpgsqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@email", user.Email);
         cmd.Parameters.AddWithValue("@phone_num", user.PhoneNum);
          cmd.Parameters.AddWithValue("@isactive", user.IsActive);
           cmd.Parameters.AddWithValue("@receiveemailnotification", user.ReceiveEmailNotification);
            cmd.Parameters.AddWithValue("@receivesmsnotification", user.ReceiveSmsNotification);
            cmd.Parameters.AddWithValue("@createdat", DateTime.Now);

           user.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }


    public bool Exists(int id)
    {
       using var conn = _dbConnection.GetConnection();
        string query = "SELECT COUNT(1) FROM users WHERE id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool ExistsByEmail(string email)
    {
       using var conn = _dbConnection.GetConnection();
        string query = "SELECT COUNT(1) FROM users WHERE email = @email";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }


    public int GetNextId()
    {
        using var conn = _dbConnection.GetConnection();
        string query = "select coalesce(max(id) , 0) + 1 from users";
        var cmd = new NpgsqlCommand(query, conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    */

}
}
