using System;
using System.Data.Common;

namespace WordGuessingGame.Models
{
    public class GameEntity
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public int WordId {get; set;}
        public string SecretWord {get; set;} = string.Empty;
        public string Difficulty {get; set;} = string.Empty;
        public int MaxAttempts {get; set;}
        public int AttemptsUsed {get; set;}
        public Boolean IsWon {get; set;} = false;
        public int Score { get; set;}
        public string? GuessesText {get; set;}
        public DateTime PlayedAt {get; set;} = DateTime.Now;
    }
}