using Onitama.Core.PlayerAggregate;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.TableAggregate.Contracts;
using Onitama.Core.UserAggregate;
using Onitama.Core.Util;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Onitama.Core.TableAggregate;

/// <inheritdoc cref="ITable"/>
internal class Table : ITable
{
    private static readonly Color[] PossibleColors =
        new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Orange };

    public List<HumanPlayer> _seatedPlayer = new List<HumanPlayer>(2);

    public Table(Guid tabelId , TablePreferences preferences)
    {
        Id = tabelId;
        Preferences = preferences;
        HasAvailableSeat = true;
    }
    public Guid Id { get; set; }

    public TablePreferences Preferences { get; set; }

    public Guid OwnerPlayerId { get; set; }


    public IReadOnlyList<IPlayer> SeatedPlayers 
    { 
        get =>_seatedPlayer;
    }

    public bool HasAvailableSeat { get; set; }

    public Guid GameId { get; set; }

    public void FillWithArtificialPlayers(IGamePlayStrategy gamePlayStrategy)
    {
        throw new NotImplementedException();
    }

    public void Join(User user)
    {
        Random randColor = new Random();
        foreach (HumanPlayer humanPlayer in _seatedPlayer)
        {
            if(humanPlayer.Id == user.Id)
            {
                throw new InvalidOperationException("already seated");
            }
        }
        if (HasAvailableSeat)
        {
            if (_seatedPlayer.Count == 0)
            {
                _seatedPlayer.Add(new HumanPlayer(user.Id, user.WarriorName, PossibleColors[randColor.Next(0, 5)], Direction.North));
                OwnerPlayerId = user.Id;
            }
            else
            {
                Color color = PossibleColors[randColor.Next(0, 5)];
                bool found = false;
                while (!found)
                {
                    color = PossibleColors[randColor.Next(0, 5)];
                    if (color != SeatedPlayers[0].Color)
                    {
                        found = true;
                    }
                }
                Direction direction = SeatedPlayers[0].Direction == Direction.South ? Direction.North : Direction.South;
                _seatedPlayer.Add(new HumanPlayer(user.Id, user.WarriorName, color, direction));
                HasAvailableSeat = false;
            }
        } else
        {
            throw new InvalidOperationException("Table is full");
        }

        if (SeatedPlayers[0].Direction == Direction.South)
        {
            (_seatedPlayer[0], _seatedPlayer[1]) = (_seatedPlayer[1], _seatedPlayer[0]);
        }
    }

    public void Leave(Guid userId)
    {
        IPlayer player = SeatedPlayers.FirstOrDefault(player => player != null && player.Id == userId) ?? throw new InvalidOperationException();

        _seatedPlayer.Remove((HumanPlayer)player);
              
        if(SeatedPlayers.Count > 0) OwnerPlayerId = SeatedPlayers[0].Id;
        HasAvailableSeat = true;
    }
}