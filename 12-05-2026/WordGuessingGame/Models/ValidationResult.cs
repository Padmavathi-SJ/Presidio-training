using System;

namespace WordGuessingGame.Models{
    public class ValidationResult{
        
        public bool isValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult{
            isValid = true
        };

        public static ValidationResult Failure(string errorMessage) => new ValidationResult{
            isValid = false,
            ErrorMessage = errorMessage
        };
    }
}