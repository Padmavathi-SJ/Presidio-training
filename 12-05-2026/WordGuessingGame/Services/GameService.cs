using System;
using WordGuessingGame.Models;
using WordGuessingGame.Repositories;
using WordGuessingGame.Database;

namespace WordGuessingGame.Services
{
    public class GameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IWordRepository _wordRepository;
         private readonly IUserRepository _userRepository; 

        public GameService(IGameRepository gameRepository, IWordRepository wordRepository, IUserRepository userRepository)
        {
            _gameRepository = gameRepository;
            _wordRepository = wordRepository;
            _userRepository = userRepository;
        }

        public void SaveGameResult(int userId, string secretWord, string difficulty, int maxAttempts, int attemptsUsed, bool isWon, int score, string guessesText)
        {

            string normalizedWord = secretWord.ToUpper();

            // get word id from database
            var word = _wordRepository.GetWordByValue(secretWord);
            int wordId = word?.Id ?? 0;

                if (wordId == 0)
    {
         var newWord = new WordEntity
        {
            Word = secretWord,
            Difficulty = difficulty,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        _wordRepository.Add(newWord);
        wordId = newWord.Id;
    }
            var game = new GameEntity
            {
                UserId = userId,
                 WordId = wordId,
                SecretWord = secretWord,
                Difficulty = difficulty,
                MaxAttempts = maxAttempts,
                AttemptsUsed = attemptsUsed,
                IsWon = isWon,
                Score = score,
                GuessesText = guessesText,
                PlayedAt = DateTime.Now
            };

            _gameRepository.Add(game);
        }

        public void DisplayGameHistory(int userId)
        {
            var games = _gameRepository.GetUserGameHistory(userId, 10);

            if(games.Count == 0)
            
                Console.WriteLine("No game history found.");
            
            foreach(var game in games)
            {
                string result = game.IsWon ? "Won" : "Lost";
                Console.WriteLine($"{game.PlayedAt:yyyy-MM-dd HH:mm,-20} {game.SecretWord, -10} {game.Difficulty, -10} {game.AttemptsUsed, -10} {game.Score, -8} {result, -8}");
            }
        }

public void DisplayLeaderboard(int limit = 10)
{
    var topScores = _gameRepository.GetTopScores(limit);
    
    if (topScores.Count == 0)
    {
        Console.WriteLine("\n📭 No scores available.");
        return;
    }

    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    
    Console.WriteLine("LEADERBOARD");
   
    Console.ResetColor();
    
   
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{"Rank",-6} {"User ID",-10} {"Score",-10} {"Word",-12} {"Attempts",-10}");
    Console.WriteLine(new string('-', 55));
    Console.ResetColor();

    int rank = 1;
    foreach (var game in topScores)
    {
        Console.WriteLine($"{rank,-6} {game.UserId,-10} {game.Score,-10} {game.SecretWord,-12} {game.AttemptsUsed,-10}");
        rank++;
    }
    Console.WriteLine();
}
    
    
    }
}