using System;
using System.Collections.Generic;
using Npgsql;
using WordGuessingGame.Models;
using WordGuessingGame.Database;

namespace WordGuessingGame.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public UserRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public List<UserEntity> GetAll()
        {
            var users = new List<UserEntity>();

            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, name, password, created_at FROM users ORDER BY id";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Password = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }
            return users;
        }

        public UserEntity? GetById(int id)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, name, password, created_at FROM users WHERE id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Password = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };
            }
            return null;
        }

        public UserEntity? GetByName(string name)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, name, password, created_at FROM users WHERE name = @name";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Password = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };
            }
            return null;
        }

        public void Add(UserEntity user)
        {
            using var conn = _dbConnection.GetConnection();
            string query = @"
                INSERT INTO users (name, password, created_at) 
                VALUES (@name, @password, @created_at)
                RETURNING id";
            
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
            
            user.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool ExistsByName(string name)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT COUNT(1) FROM users WHERE name = @name";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public UserEntity? Authenticate(string name, string password)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, name, password, created_at FROM users WHERE name = @name AND password = @password";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", password);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Password = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };
            }
            return null;
        }
    }
}