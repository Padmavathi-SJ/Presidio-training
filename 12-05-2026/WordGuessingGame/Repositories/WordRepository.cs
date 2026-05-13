using System;
using System.Collections.Generic;
using Npgsql;
using WordGuessingGame.Models;
using WordGuessingGame.Database;

namespace WordGuessingGame.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public WordRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public List<WordEntity> GetAll()
        {
            var words = new List<WordEntity>();
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, word, difficulty, is_active, created_at FROM words ORDER BY id";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                words.Add(new WordEntity
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1),
                    Difficulty = reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }
            return words;
        }

        public WordEntity? GetById(int id)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, word, difficulty, is_active, created_at FROM words WHERE id = @id AND is_active = true";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new WordEntity
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1),
                    Difficulty = reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }
            return null;
        }

        public WordEntity? GetWordByValue(string word)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT id, word, difficulty, is_active, created_at FROM words WHERE word = @word AND is_active = true";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@word", word);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new WordEntity
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1),
                    Difficulty = reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }
            return null;
        }

        public WordEntity? GetRandomWord(string? difficulty = null)
        {
            using var conn = _dbConnection.GetConnection();
            string query;

            if (string.IsNullOrWhiteSpace(difficulty))
            {
                query = "SELECT id, word, difficulty, is_active, created_at FROM words WHERE is_active = true ORDER BY RANDOM() LIMIT 1";
            }
            else
            {
                query = "SELECT id, word, difficulty, is_active, created_at FROM words WHERE difficulty = @difficulty AND is_active = true ORDER BY RANDOM() LIMIT 1";
            }

            using var cmd = new NpgsqlCommand(query, conn);
            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                cmd.Parameters.AddWithValue("@difficulty", difficulty.ToLower());
            }

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new WordEntity
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1).ToUpper(),
                    Difficulty = reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }
            return null;
        }

        public void Add(WordEntity word)
        {
            using var conn = _dbConnection.GetConnection();
            string query = @"
                INSERT INTO words (word, difficulty, is_active, created_at) 
                VALUES (@word, @difficulty, @is_active, @created_at)
                RETURNING id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@word", word.Word.ToUpper());
            cmd.Parameters.AddWithValue("@difficulty", word.Difficulty);
            cmd.Parameters.AddWithValue("@is_active", word.IsActive);
            cmd.Parameters.AddWithValue("@created_at", DateTime.Now);

            word.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Exists(string word)
        {
            using var conn = _dbConnection.GetConnection();
            string query = "SELECT COUNT(1) FROM words WHERE word = @word";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@word", word.ToUpper());
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}