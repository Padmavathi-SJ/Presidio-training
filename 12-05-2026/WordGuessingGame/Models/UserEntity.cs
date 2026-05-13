using System;

namespace WordGuessingGame.Models
{
    public class UserEntity
    {
        public int Id {get;set;}
        public string Name {get; set;} = string.Empty;
        public string Password {get; set;} = string.Empty;
        public DateTime CreatedAt {get; set;} = DateTime.Now;
    }
}