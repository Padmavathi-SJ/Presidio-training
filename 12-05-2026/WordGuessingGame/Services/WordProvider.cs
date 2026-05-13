using System;
using System.Collections.Generic;
using WordGuessingGame.Repositories;
using WordGuessingGame.Database;

namespace WordGuessingGame.Services{
    public class WordProvider{
        private readonly WordRepository _wordRepository;

        public WordProvider(DatabaseConnection dbConnection)
        {
            _wordRepository = new WordRepository(dbConnection);

        }

        public string GetRandomWord(string? difficulty = null)
        {
            var word = _wordRepository.GetRandomWord(difficulty);
            return word?.Word ?? "APPLE";
        }

        public (string word, int wordId) GetRandomWordWithId(string? difficulty = null)
        {
            var word = _wordRepository.GetRandomWord(difficulty);
            return (word?.Word ?? "APPLE", word?.Id ?? 1);
        }
    }

}