using System;
using WordGuessingGame.Models;
using WordGuessingGame.Database;
using Npgsql;
using WordGuessingGame.Services;

namespace WordGuessingGame.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly DatabaseConnection _dbConnection;
        public GameRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

       public GameEntity Add(GameEntity game)
        {
            using var conn = _dbConnection.GetConnection();
            string query = @"
            insert into games (user_id, word_id, secret_word, difficulty, max_attempts, attempts_used, is_won, score, guesses_text, played_at)
            values (@user_id, @word_id, @secret_word, @difficulty, @max_attempts, @attempts_used, @is_won, @score, @guesses_text, @played_at)
            returning id
            ";

             using var cmd = new NpgsqlCommand(query, conn);

             cmd.Parameters.AddWithValue("@user_id", game.UserId);
             cmd.Parameters.AddWithValue("@word_id", game.WordId);
             cmd.Parameters.AddWithValue("@secret_word", game.SecretWord);
             cmd.Parameters.AddWithValue("@difficulty", game.Difficulty);
             cmd.Parameters.AddWithValue("@max_attempts", game.MaxAttempts);
             cmd.Parameters.AddWithValue("@attempts_used", game.AttemptsUsed);
             cmd.Parameters.AddWithValue("@is_won", game.IsWon);
             cmd.Parameters.AddWithValue("@score", game.Score);
             cmd.Parameters.AddWithValue("@guesses_text", game.GuessesText);
             cmd.Parameters.AddWithValue("@played_at", game.PlayedAt);

             game.Id = Convert.ToInt32(cmd.ExecuteScalar());
             return game;
        }

        public GameEntity? GetById(int id)
        {
            var games = new List<GameEntity>();
            using var conn = _dbConnection.GetConnection();
            string query = @"
            select id, user_id, word_id, secret_word, difficulty, max_attempts, attempts_used, is_won, score, guesses_text, played_at 
            from games where id = @id
            ";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                games.Add(new GameEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    WordId = reader.GetInt32(2),
                    SecretWord = reader.GetString(3),
                    Difficulty = reader.GetString(4),
                    MaxAttempts = reader.GetInt32(5),
                    AttemptsUsed = reader.GetInt32(6),
                    IsWon = reader.GetBoolean(7),
                    Score = reader.GetInt32(8),
                    GuessesText = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PlayedAt = reader.GetDateTime(10)
                });
            }
            return null;
        }

        public List<GameEntity> GetByUserId(int userId)
        {
            var games = new List<GameEntity>();
             using var conn = _dbConnection.GetConnection();
            string query = @"
        SELECT id, user_id, word_id, secret_word, difficulty, max_attempts, attempts_used, is_won, score, guesses_text, played_at 
        FROM games WHERE user_id = @user_id 
        ORDER BY played_at DESC";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_id", userId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
               games.Add(new GameEntity
               {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    WordId = reader.GetInt32(2),
                    SecretWord = reader.GetString(3),
                    Difficulty = reader.GetString(4),
                    MaxAttempts = reader.GetInt32(5),
                    AttemptsUsed = reader.GetInt32(6),
                    IsWon = reader.GetBoolean(7),
                    Score = reader.GetInt32(8),
                    GuessesText = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PlayedAt = reader.GetDateTime(10)
                });
            }
            return games;

        }

        public void Update(GameEntity game)
        {
            using var conn = _dbConnection.GetConnection();
            string query = @"
            update games set attempts_used = @attempts_used, is_won = @is_won, score = @score, guesses_text = @guesses_text
            where id = @id
            ";
            using var cmd = new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@attempts_used", game.AttemptsUsed);
            cmd.Parameters.AddWithValue("@is_won", game.IsWon);
            cmd.Parameters.AddWithValue("@score", game.Score);
            cmd.Parameters.AddWithValue("@guesses_text", game.GuessesText ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@id", game.Id);

            cmd.ExecuteNonQuery();
        }

        public List<GameEntity> GetTopScores(int limit = 10)
        {
            var games = new List<GameEntity>();

            using var conn = _dbConnection.GetConnection();
            string query = @"
        SELECT g.id, g.user_id, g.word_id, g.secret_word, g.difficulty, g.max_attempts, g.attempts_used, g.is_won, g.score, g.guesses_text, g.played_at, u.name
        FROM games g 
        JOIN users u ON u.id = g.user_id
        WHERE g.is_won = true
        ORDER BY g.score DESC 
        LIMIT @limit";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@limit", limit);

              using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                games.Add(new GameEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    WordId = reader.GetInt32(2),
                    SecretWord = reader.GetString(3),
                    Difficulty = reader.GetString(4),
                    MaxAttempts = reader.GetInt32(5),
                    AttemptsUsed = reader.GetInt32(6),
                    IsWon = reader.GetBoolean(7),
                    Score = reader.GetInt32(8),
                    GuessesText = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PlayedAt = reader.GetDateTime(10)
                });
            }
            return games;
        }

public List<GameEntity> GetUserGameHistory(int userId, int limit = 20)
        {
            var games = new List<GameEntity>();
            using var conn = _dbConnection.GetConnection();
            string query = @"
                SELECT id, user_id, word_id, secret_word, difficulty, max_attempts, attempts_used, is_won, score, guesses_text, played_at 
                FROM games 
                WHERE user_id = @user_id 
                ORDER BY played_at DESC 
                LIMIT @limit";
            
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@limit", limit);
            using var reader = cmd.ExecuteReader();

              while (reader.Read())
            {
                games.Add(new GameEntity
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    WordId = reader.GetInt32(2),
                    SecretWord = reader.GetString(3),
                    Difficulty = reader.GetString(4),
                    MaxAttempts = reader.GetInt32(5),
                    AttemptsUsed = reader.GetInt32(6),
                    IsWon = reader.GetBoolean(7),
                    Score = reader.GetInt32(8),
                    GuessesText = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PlayedAt = reader.GetDateTime(10)
                });
            }
            return games;
        }

        
    }
    }
