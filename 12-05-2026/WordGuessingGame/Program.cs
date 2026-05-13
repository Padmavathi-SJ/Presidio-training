using System;
using System.Collections.Generic;
using System.Linq;
using DotNetEnv;
using WordGuessingGame.Exceptions;
using WordGuessingGame.Models;
using WordGuessingGame.Services;
using WordGuessingGame.Repositories;
using WordGuessingGame.Database;
using WordGuessingGame.Config;

namespace WordGuessingGame
{
    class Program
    {
        private static AuthService _authService = null!;
        private static GameService _gameService = null!;
        private static WordProvider _wordProvider = null!;
        private static GuessValidator _validator = null!;
        private static FeedbackGenerator _feedbackGenerator = null!;
        private static DatabaseConnection _dbConnection = null!;
        private static UserRepository _userRepository = null!;
        private static WordRepository _wordRepository = null!;
        private static GameRepository _gameRepository = null!;

        static void Main(string[] args)
        {
            Console.Title = "Word Guessing Game";

            Env.Load();

            // Initialize Database Configuration
            var dbConfig = new DatabaseConfig
            {
                Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
                DatabaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "wordguessinggame",
                UserName = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
            };

            _dbConnection = new DatabaseConnection(dbConfig);

             try
            {
                using var conn = _dbConnection.GetConnection();
                Console.WriteLine("Database connected successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
             
                return;
            }

            // Initialize Repositories
            _userRepository = new UserRepository(_dbConnection);
            _wordRepository = new WordRepository(_dbConnection);
            _gameRepository = new GameRepository(_dbConnection);

            // Initialize Services
            _authService = new AuthService(_userRepository);
            _gameService = new GameService(_gameRepository, _wordRepository, _userRepository);
            _wordProvider = new WordProvider(_dbConnection);
            _validator = new GuessValidator();
            _feedbackGenerator = new FeedbackGenerator();

            // Show welcome screen
            DisplayWelcomeScreen();

            // Login/Register loop
            bool authenticated = false;
            while (!authenticated)
            {
                DisplayAuthMenu();
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        Login();
                        authenticated = _authService.IsLoggedIn;
                        break;
                    case "2":
                        Register();
                        break;
                    case "3":
                        Console.WriteLine("\nThanks for visiting! Goodbye!");
                        return;
                    default:
                        Console.WriteLine("\nInvalid option! Please try again.");
                        break;
                }
            }

            // Main game loop
            bool playing = true;
            int sessionGames = 0;
            int sessionWins = 0;
            int sessionScore = 0;

            while (playing)
            {
                DisplayMainMenu();
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        // Play Game
                        var result = PlayGame();
                        if (result != null)
                        {
                            sessionGames++;
                            if (result.IsWon)
                            {
                                sessionWins++;
                                sessionScore += result.Score;
                            }
                            // Save to database
                            _gameService.SaveGameResult(
                                _authService.CurrentUser!.Id,
                                result.SecretWord,
                                result.Attempts <= 3 ? "hard" : (result.Attempts <= 5 ? "medium" : "easy"),
                                6,
                                result.Attempts,
                                result.IsWon,
                                result.Score,
                                "" // Guesses text can be stored if needed
                            );
                            Console.WriteLine("\nGame result saved to database!");
                        }
                        DisplaySessionStats(sessionGames, sessionWins, sessionScore);
                        break;
                    case "2":
                        _gameService.DisplayGameHistory(_authService.CurrentUser!.Id);
                        break;
                    case "3":
                        _gameService.DisplayLeaderboard();
                        break;
                    case "4":
                        ShowUserProfile();
                        break;
                    case "5":
                        playing = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid option! Please try again.");
                        break;
                }

                if (playing && choice != "1")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
                Console.Clear();
            }

            Console.WriteLine($"\nGoodbye {_authService.CurrentUserName}! Thanks for playing!");
        }

        static void DisplayWelcomeScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
          
            Console.WriteLine(" WORD GUESSING GAME ");

            Console.ResetColor();
            Console.WriteLine("\nRULES:");
            Console.WriteLine("Guess the 5-letter word");
            Console.WriteLine("You have 6 attempts");
            Console.WriteLine("G = Correct letter, correct position");
            Console.WriteLine("Y = Correct letter, wrong position");
            Console.WriteLine("X = Letter not in the word\n");
        }

        static void DisplayAuthMenu()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
          
            Console.WriteLine("AUTHENTICATION");
         
            Console.ResetColor();
            Console.WriteLine("\n1 Login");
            Console.WriteLine("2. Register New Account");
            Console.WriteLine("3. Exit");
            Console.Write("\nChoose an option: ");
        }

        static void DisplayMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
        
            Console.WriteLine("MAIN MENU");
            
            Console.ResetColor();
            Console.WriteLine($"\nWelcome, {_authService.CurrentUserName}!\n");
            Console.WriteLine("1. Play Game");
            Console.WriteLine("2. View My Game History");
            Console.WriteLine("3. View Leaderboard");
            Console.WriteLine("4. My Profile");
            Console.WriteLine("5. Logout");
            Console.Write("\nChoose an option: ");
        }

        static void Register()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
           
            Console.WriteLine("REGISTER NEW ACCOUNT");
          
            Console.ResetColor();

            Console.Write("\nChoose a username: ");
            string? name = Console.ReadLine();

            Console.Write("Enter password: ");
            string? password = ReadPassword();

            Console.Write("Confirm password: ");
            string? confirmPassword = ReadPassword();

            var result = _authService.Register(name ?? "", password ?? "", confirmPassword ?? "");

            if (result.success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{result.message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to login...");
                Console.ReadKey();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{result.message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey();
            }
        }

        static void Login()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
           
            Console.WriteLine("                    LOGIN");
           
            Console.ResetColor();

            Console.Write("\nUsername: ");
            string? name = Console.ReadLine();

            Console.Write("Password: ");
            string? password = ReadPassword();

            var result = _authService.Login(name ?? "", password ?? "");

            if (result.success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{result.message}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{result.message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey();
            }
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        static GameResult? PlayGame()
        {
            int maxAttempts = SelectDifficulty();
            var game = new Game(_wordProvider, _validator, _feedbackGenerator);
            
            // Override max attempts based on difficulty
            game.SetMaxAttempts(maxAttempts);
            game.StartNewGame();
            
            Console.WriteLine($"\n🎯 You have {game.RemainingAttempts} attempts to guess the 5-letter word!\n");

            while (!game.isGameOver)
            {
                try
                {
                    Console.Write($"📝 Attempt {game.AttemptsUsed + 1}: ");
                    string guess = Console.ReadLine()?.Trim().ToUpper() ?? "";

                    if (string.IsNullOrEmpty(guess))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Input cannot be empty!\n");
                        Console.ResetColor();
                        continue;
                    }

                    var result = game.MakeGuess(guess);
                    DisplayFeedback(guess, result.Feedback);

                    if (result.IsWon)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n{result.Comment}");
                        Console.WriteLine($"You scored: {result.Score} points!");
                        Console.ResetColor();
                        return result;
                    }

                    if (result.IsGameOver)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nGAME OVER! The word was: {result.SecretWord}");
                        Console.ResetColor();
                        return result;
                    }

                    Console.WriteLine($"Remaining attempts: {game.RemainingAttempts}\n");
                }
                catch (InvalidGuessException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{ex.Message}\n");
                    Console.ResetColor();
                }
            }
            return null;
        }

        static int SelectDifficulty()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
          
            Console.WriteLine(" SELECT DIFFICULTY ");
           
            Console.ResetColor();

            Console.WriteLine("\n1. EASY   - 8 attempts (Score x1.0)");
            Console.WriteLine("2. MEDIUM - 6 attempts (Score x1.5)");
            Console.WriteLine("3. HARD   - 4 attempts (Score x2.0)");
            Console.Write("\nChoose difficulty (1-3): ");

            string choice = Console.ReadLine() ?? "2";

            return choice switch
            {
                "1" => 8,
                "3" => 4,
                _ => 6
            };
        }

        static void DisplayFeedback(string guess, string feedback)
        {
            if (string.IsNullOrEmpty(guess)) return;

            Console.Write("Feedback: ");
            for (int i = 0; i < guess.Length; i++)
            {
                Console.ForegroundColor = feedback[i] switch
                {
                    'G' => ConsoleColor.Green,
                    'Y' => ConsoleColor.Yellow,
                    'X' => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };
                Console.Write($"{guess[i]}({feedback[i]}) ");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        static void DisplaySessionStats(int games, int wins, int score)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
           
            Console.WriteLine("SESSION STATISTICS");

            Console.ResetColor();

            double winRate = games > 0 ? (wins * 100.0 / games) : 0;
            
            Console.WriteLine($"\nGames Played: {games}");
            Console.WriteLine($"Games Won: {wins}");
            Console.WriteLine($"Win Rate: {winRate:F1}%");
            Console.WriteLine($"Total Score: {score}");
        }

        static void ShowUserProfile()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
          
            Console.WriteLine("USER PROFILE");
            
            Console.ResetColor();

            Console.WriteLine($"\n Username: {_authService.CurrentUser!.Name}");
            Console.WriteLine($"Member Since: {_authService.CurrentUser.CreatedAt:yyyy-MM-dd}");
            Console.WriteLine($"User ID: {_authService.CurrentUser.Id}");
            
            // Get user stats from game history
            var games = _gameRepository.GetByUserId(_authService.CurrentUser.Id);
            int totalGames = games.Count;
            int totalWins = games.Count(g => g.IsWon);
            int totalScore = games.Sum(g => g.Score);
            double winRate = totalGames > 0 ? (totalWins * 100.0 / totalGames) : 0;

            Console.WriteLine($"\n Statistics:");
            Console.WriteLine($" Total Games: {totalGames}");
            Console.WriteLine($" Games Won: {totalWins}");
            Console.WriteLine($" Win Rate: {winRate:F1}%");
            Console.WriteLine($" Total Score: {totalScore}");
        }
    }
}