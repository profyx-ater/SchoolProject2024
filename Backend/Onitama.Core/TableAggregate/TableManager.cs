using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.TableAggregate.Contracts;
using Onitama.Core.UserAggregate;

namespace Onitama.Core.TableAggregate;

/// <inheritdoc cref="ITableManager"/>
internal class TableManager : ITableManager
{
    ITableRepository _tableRepository;
    ITableFactory _tableFactory;
    IGameRepository _gameRepository;
    IGameFactory _gameFactory;
    IGamePlayStrategy _gamePlayStrategy;
    public TableManager(
        ITableRepository tableRepository,
        ITableFactory tableFactory,
        IGameRepository gameRepository,
        IGameFactory gameFactory,
        IGamePlayStrategy gamePlayStrategy)
    {
        _tableRepository = tableRepository;
        _tableFactory = tableFactory;
        _gameRepository = gameRepository;
        _gameFactory = gameFactory;
        _gamePlayStrategy = gamePlayStrategy;

    }

    public ITable AddNewTableForUser(User user, TablePreferences preferences)
    {
        ITable table;
        table = _tableFactory.CreateNewForUser(user, preferences);
        _tableRepository.Add(table);

        return table;
    }

    public void JoinTable(Guid tableId, User user)
    {
        ITable table;
        table = _tableRepository.Get(tableId);
        table.Join(user);
    }

    public void LeaveTable(Guid tableId, User user)
    {
        ITable table = _tableRepository.Get(tableId);
        if (table.SeatedPlayers.Count == 1)
        {
            table.Leave(user.Id);
            _tableRepository.Remove(tableId);
        }
        else
        {
            table.Leave(user.Id);
        }
    }

    public void FillWithArtificialPlayers(Guid tableId, User user)
    {
        throw new NotImplementedException();
    }

    public IGame StartGameForTable(Guid tableId, User user)
    {
        ITable table = _tableRepository.Get(tableId);

        if (table.HasAvailableSeat)
        {
            throw new InvalidOperationException("Not enough players to start the game");

        }
        else if (user.Id != table.OwnerPlayerId)
        {
            throw new InvalidOperationException("You are not the owner of the table");
        }

        IGame game = _gameFactory.CreateNewForTable(table);
        _gameRepository.Add(game);
        table.GameId = game.Id;
        return game;

    }
}
