using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.PlayerAggregate;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.SchoolAggregate;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.GameAggregate;

internal class GameService : IGameService
{
    private IGameRepository _gameRepository;
    private IGame _game;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public IReadOnlyList<IMove> GetAllPossibleMovesFromOpponent(Guid gameId, Guid userId)
    {
        _game = GetGame(gameId);
        return _game.GetAllPossibleMovesFromOpponent(userId);
    }

    public IGame GetGame(Guid gameId)
    {
        return _gameRepository.GetById(gameId);
    }

    public IReadOnlyList<IMove> GetPossibleMovesForPawn(Guid gameId, Guid playerId, Guid pawnId, string moveCardName)
    {
        _game = GetGame(gameId);
        return _game.GetPossibleMovesForPawn(playerId, pawnId, moveCardName);
    }

    public void MovePawn(Guid gameId, Guid playerId, Guid pawnId, string moveCardName, ICoordinate to)
    {
        _game = GetGame(gameId);
        _game.MovePawn(playerId, pawnId, moveCardName, to);
    }

    public void SkipMovementAndExchangeCard(Guid gameId, Guid playerId, string moveCardName)
    {
        _game = GetGame(gameId);
        _game.SkipMovementAndExchangeCard(playerId, moveCardName);
    }
}