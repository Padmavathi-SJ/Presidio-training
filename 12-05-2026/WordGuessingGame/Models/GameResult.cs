using System;

namespace WordGuessingGame.Models{
    
    public class GameResult{
        public bool IsGameOver { get; set; }
        public bool IsWon { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string Comment { get; set; }= string.Empty;
        public int Attempts { get; set; }
        public string SecretWord { get; set; }
        public int Score { get; set; }

        public  GameResult(bool isGameOver, bool isWon, string feedback, string comment, int attempts , string secret_word, int score){
            IsGameOver = isGameOver;
            IsWon = isWon;
            Feedback = feedback;
            Comment = comment;
            Attempts = attempts;
            SecretWord = secret_word;
            Score = score;

        }
    }
}