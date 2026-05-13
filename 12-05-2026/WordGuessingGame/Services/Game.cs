using System;
using System.Linq;
using System.Collections.Generic;
using WordGuessingGame.Exceptions;
using WordGuessingGame.Models;
using WordGuessingGame.Services;

namespace WordGuessingGame.Services{
    class Game{
        private readonly WordProvider _wordprovider;
        private readonly GuessValidator _guessvalidator;
        private readonly FeedbackGenerator _feedbackgenerator;

        // Game state variables
        private int _wordId;
        private string secret_word;
        private int max_attempts;
        private int attempts_used;
        private List<string> previousGuesses;
        private bool isGame_over;
        private bool is_won;

        public int AttemptsUsed => attempts_used;
        public int RemainingAttempts => max_attempts - attempts_used;
        public bool isGameOver => isGame_over;
        public bool IsWon => is_won;

        // Add this method to get word ID
public int GetWordId() => _wordId;

// Add this method to get guesses as string
public string GetGuessesText()
{
    return string.Join(",", previousGuesses);
}

// Add this method to get difficulty name
public string GetDifficultyName()
{
    return max_attempts switch
    {
        8 => "easy",
        6 => "medium",
        4 => "hard",
        _ => "medium"
    };
}

        public Game(WordProvider word_provider, GuessValidator guess_validator, FeedbackGenerator feedback_generator){
            _wordprovider = word_provider;
            _guessvalidator = guess_validator;
            _feedbackgenerator = feedback_generator;
            max_attempts = 6;
            previousGuesses = new List<string>();
            StartNewGame();
        }

        public void StartNewGame(string? difficulty = null){
            var (word, wordId) = _wordprovider.GetRandomWordWithId(difficulty);
            secret_word = word;
            _wordId = wordId;
            attempts_used = 0;
            previousGuesses.Clear();
            isGame_over = false;
            is_won = false;

        }

        private int CalculateScore()
        {
            int baseScore = (max_attempts - attempts_used + 1) * 100;
            double multiplier = max_attempts switch
            {
                8 => 1.0,
                6 => 1.5,
                4 => 2.0,
                _ => 1.0
            };
            return (int) (baseScore * multiplier);
        }

        public GameResult MakeGuess(string guess){
            var validation =  _guessvalidator.Validate(guess);

            if(!validation.isValid)
                throw new InvalidGuessException(validation.ErrorMessage);

            string normalized_guess = guess.ToUpper().Trim();

            // check duplicate
            if(previousGuesses.Contains(normalized_guess))
               throw new InvalidGuessException($"you already guessed this word {normalized_guess}!");

            previousGuesses.Add(normalized_guess);
            attempts_used++;


            // check if won
            if(normalized_guess == secret_word){
                is_won = true;
                isGame_over = true;
                string comment = GetCommentsForAttempts();
                int score = CalculateScore();
                return new GameResult(true, true, "GGGGG", comment, attempts_used, secret_word, score);
            }

            string feedback = GenerateFeedback(normalized_guess, secret_word);

            // check game over
            if(attempts_used >= max_attempts){
                isGame_over = true;
                return new GameResult(true, false, feedback, "", attempts_used, secret_word, 0);
            }

            return new GameResult(false, false, feedback, "", attempts_used, secret_word, 0);
        }

        private string GenerateFeedback(string guess, string secret){
            char[] temp = new char[5];
            for(int i = 0; i<5; i++){
                if(guess[i] == secret[i]){
                    temp[i] = 'G';
                } else if (!secret.Contains(guess[i]))
                {
                    temp[i] = 'X';
                }
                else
                {
                    temp[i] = 'Y';
                }
            }
            return new string(temp);
        }

        private string GetCommentsForAttempts()
        {
            switch (attempts_used)
            {
                case 1:
                    return "Genius!";
                case 2:
                    return "Excellent";
                case 3:
                    return "Great Job!";
                case 4:
                    return "Great work!";
                    
                case 5:
                return "nice try!";
                    
                case 6:
                return "That was close!";
                default:
                return "Good try!";
            }
        }

        public string GetSecretWord() => secret_word;

        
public void SetMaxAttempts(int attempts)
{
    max_attempts = attempts;
}

        public void DisplayPreviousGuesses()
        {
            if(previousGuesses.Count == 0)
            {
                Console.WriteLine("No guesses yet!");
                return;
            }
            Console.WriteLine("\n Your previous guesses: ");
            foreach(var guess in previousGuesses)
            {
                string feedback = GenerateFeedback(guess, secret_word);
                Console.WriteLine($" {guess} -> {feedback}");
            }
        }




    }
}