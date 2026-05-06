using System;
using System.Collections.Generic;

namespace WordGuessingGame.Services{
    public class WordProvider{
        private readonly List<string> words;
        private readonly Random random;

        public WordProvider() 
        {
            random = new Random();
            words = new List<string>{
                "CHAIR", "PHONE", "MUSIC", "WATER", "LIGHT",
                "APPLE", "MANGO", "GRAPE", "TRAIN", "PLANT"
            };
        }

        public string GetRandomWord(){
            return words[random.Next(words.Count)];
        }
    }
}