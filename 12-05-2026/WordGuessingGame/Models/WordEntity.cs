using System;

namespace WordGuessingGame.Models
{
    public class WordEntity
    {
        public int Id {get; set;}
        public string Word {get; set;} = string.Empty;
        public string Difficulty {get; set;} = "Medium";
        public Boolean IsActive {get; set;} = true;
        public DateTime CreatedAt {get; set;} = DateTime.Now;
    }
}
