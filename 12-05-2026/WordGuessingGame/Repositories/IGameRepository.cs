using System;
using WordGuessingGame.Models;
using WordGuessingGame.Services;

namespace WordGuessingGame.Repositories
{
    public interface IGameRepository
    {
      GameEntity Add(GameEntity game);
        GameEntity? GetById(int id);
        List<GameEntity> GetByUserId(int userId);
        void Update(GameEntity game);
        List<GameEntity> GetTopScores(int limit = 10);
        List<GameEntity> GetUserGameHistory(int userId, int limit = 20);

    }
}