using System;
using WordGuessingGame.Models;

namespace WordGuessingGame.Repositories
{
    public interface IWordRepository
    {
        List<WordEntity> GetAll();
        WordEntity? GetById(int id);
        WordEntity? GetWordByValue(string word);
        WordEntity? GetRandomWord (string? difficulty = null);
        void Add(WordEntity word);
        bool Exists(string word);
    }
}