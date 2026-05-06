using System;
using WordGuessingGame.Exceptions;
using WordGuessingGame.Models;
using WordGuessingGame.Services;

namespace WordGuessingGame
{
    class Program
    {
        static void Main(string[] args)
        {
            DisplayHeader();

            bool playAgain = true;
            int totalGames = 0;
            int totalwins = 0;
            int totalScores = 0;

            while (playAgain)
            {
                // select difficulty
                int maxAttempts = SelectDifficulty();

                var wordProvider = new WordProvider();
                var validator = new GuessValidator();
                var feedbackGenerator = new FeedbackGenerator();
                var game = new Game(wordProvider, validator, feedbackGenerator);

                // play game
                var result = PlayGame(game);

                // update stats
                totalGames++;
                if (result.IsWon)
                    totalwins++;
                totalScores += result.Score;

                //show stats
                DisplayStats(totalGames, totalwins, totalScores);

                playAgain = AskForPlay();

                if (playAgain)
                    Console.Clear();
            }
        }

        static void DisplayHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("WORD GUESSING GAME");
            Console.WriteLine("Rules: \n");
            Console.WriteLine("Guess the 5-letter word\n");
            Console.WriteLine("G = Correct letter, in correct position\n");
            Console.WriteLine("Y = Correct letter, but in different position\n");
            Console.WriteLine("X = Letter not present in the secret word\n");

            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
            Console.Clear();
        }

        static int SelectDifficulty() 
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("SELECT DIFFICULTY");
            Console.ResetColor();

            Console.WriteLine("\n1. EASY - 8 attempts (score 1.0)");
            Console.WriteLine("2. MEDIUM - 6 attempts (score 1.5)");
            Console.WriteLine("3. HARD - 4 attempts (score 2.0)");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nEasy mode selected! you have 8 attempts.");
                    Console.ResetColor();
                    return 8;

                case "2":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nMedium mode selected! you have 6 attempts.");
                    Console.ResetColor();
                    return 6;

                case "3":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nHard mode selected! you have 4 attempts.");
                    Console.ResetColor();
                    return 4;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nMedium mode selected! you have 6 attempts.");
                    Console.ResetColor();
                    return 6;
            }
        }

        static GameResult PlayGame(Game game)
        {
            Console.WriteLine($"\nAttempts: {game.RemainingAttempts}\n");
            while (!game.isGameOver)
            {
                try
                {
                    Console.Write($"Guess #{game.AttemptsUsed + 1}: ");
                    string guess = Console.ReadLine()?.Trim().ToUpper();

                    if (string.IsNullOrEmpty(guess)) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Input cannot be empty!\n");
                        Console.ResetColor();
                        continue;
                    }

                    var result = game.MakeGuess(guess);

                    // Display feedback with colors
                    DisplayFeedback(guess, result.Feedback);

                    // check win
                    if (result.IsWon)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n{result.Comment} You won in {result.Attempts} attempts! +{result.Score} points");
                        Console.ResetColor();
                        return result;
                    }

                    // check loss
                    if (result.IsGameOver)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nGame Over! The word was: {result.SecretWord}\n");
                        Console.ResetColor();
                        return result;
                    }
                    Console.WriteLine($"Remaining attempts: {game.RemainingAttempts}\n");
                }
                catch (InvalidGuessException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}\n");
                    Console.ResetColor();
                }
            }
            return new GameResult(true, false, "", "", 0, "", 0);
        }

        static void DisplayFeedback(string guess, string feedback)
        {
            if (string.IsNullOrEmpty(guess)) return;

            Console.Write("Feedback: ");
            for (int i = 0; i < guess.Length; i++)
            {
                switch (feedback[i])
                {
                    case 'G':
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case 'Y':
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case 'X':
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Console.Write($"{guess[i]}({feedback[i]}) "); 
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        static void DisplayStats(int totalGames, int totalWins, int totalScores) 
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nSTATISTICS");
            Console.ResetColor();

            Console.WriteLine($"Games Played: {totalGames}");
            Console.WriteLine($"Games Won: {totalWins}");
            Console.WriteLine($"Total Score: {totalScores}");
            Console.WriteLine($"Win Rate: {(totalGames > 0 ? (totalWins * 100.0 / totalGames) : 0):F1}%");
        }

        static bool AskForPlay()
        {
            Console.Write("\nPlay again? (Y/N): ");
            return Console.ReadLine()?.ToUpper() == "Y";
        }
    }
}