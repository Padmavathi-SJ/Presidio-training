using System;
using System.Text.RegularExpressions;
using WordGuessingGame.Models;
using WordGuessingGame.Exceptions;

namespace WordGuessingGame.Services{

    public class GuessValidator{
        private const int expected_word_length=5;

        public ValidationResult Validate(string guess){
            try{
                // check for empty string
                if(string.IsNullOrWhiteSpace(guess))
                    throw new InvalidGuessException("Input cannot be empty");
                
                guess = guess.ToUpper().Trim();

                if(guess.Length < expected_word_length)
                    throw new InvalidGuessException($"word length must be {expected_word_length} letters. your guess word has only {guess.Length} letter.");
                
                if(Regex.IsMatch(guess, @"\d"))
                    throw new InvalidGuessException("Numbers are not allowed, please give a word with only letters.");

                if(Regex.IsMatch(guess, @"[^A-Z]"))
                    throw new InvalidGuessException("Special characters are not allowed, please give a word with only letters");
            
            return ValidationResult.Success();
            }
            catch(Exception ex){
                return ValidationResult.Failure($"Validation error: {ex.Message}");
            }

        }
    }
}