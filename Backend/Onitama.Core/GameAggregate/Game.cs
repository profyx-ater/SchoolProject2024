using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.PlayMatAggregate.Contracts;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Onitama.Core.GameAggregate;

/// <inheritdoc cref="IGame"/>
internal class Game : IGame
{
    public Game(Guid id, IPlayMat playMat, IPlayer[] players, IMoveCard extraMoveCard)
    {
        Id = id;
        Players = players;
        PlayMat = playMat;
        ExtraMoveCard = extraMoveCard;
        PlayerToPlayId = ExtraMoveCard.StampColor == Players[0].Color ? Players[0].Id : Players[1].Id;

        foreach (var player in Players)
        {
            PlayMat.PositionSchoolOfPlayer(player);
        }
    }

    public Guid Id { get; set; }

    public IPlayMat PlayMat { get; set; }

    public IMoveCard ExtraMoveCard { get; set; }

    public IPlayer[] Players { get; set; }

    public Guid PlayerToPlayId { get; set; }

    public Guid WinnerPlayerId { get; set; }

    public string WinnerMethod { get; set; }

    /// <summary>
    /// Creates a new game and determines the player to play first.
    /// </summary>
    /// <param name="id">The unique identifier of the game</param>
    /// <param name="playMat">
    /// The play mat
    /// (with the schools of the player already positioned on it)
    /// </param>
    /// <param name="players">
    /// The 2 players that will play the game
    /// (with 2 move cards each)
    /// </param>
    /// <param name="extraMoveCard">
    /// The fifth card used to exchange cards after the first move
    /// </param>

    /// <summary>
    /// Creates a game that is a copy of another game.
    /// </summary>
    /// <remarks>
    /// This is an EXTRA. Not needed to implement the minimal requirements.
    /// To make the mini-max algorithm for an AI game play strategy work, this constructor should be implemented.
    /// </remarks>
    public Game(IGame otherGame)
    {
        throw new NotImplementedException("TODO: copy the properties of the other game");
        //Attention: the players should be copied, not just referenced
    }

    public IReadOnlyList<IMove> GetPossibleMovesForPawn(Guid playerId, Guid pawnId, string moveCardName)
    {
        IPlayer player = Players.FirstOrDefault(p => p.Id == playerId) ?? throw new InvalidOperationException("Player not found");
        IMoveCard moveCard = player.MoveCards.FirstOrDefault(mc => mc.Name == moveCardName) ?? throw new ApplicationException("This is not your card");
        IPawn pawn = player.School.AllPawns.First(pawn => pawn.Id == pawnId);

        IReadOnlyList<IMove> validMoves = PlayMat.GetValidMoves(pawn, moveCard, player.Direction);

        return validMoves;

    }
    public IReadOnlyList<IMove> GetAllPossibleMovesFromOpponent(Guid playerId)
    {
        Guid opponentId;
        if (Players[0].Id == playerId)
        {
            opponentId = Players[1].Id;  
        }
        else
        {
            opponentId = Players[0].Id;
        }
        return GetAllPossibleMovesFor(opponentId);
    }
    public IReadOnlyList<IMove> GetAllPossibleMovesFor(Guid playerId)
    {
        IPlayer currentPlayer = Players.FirstOrDefault(p => p.Id == playerId) ?? throw new ApplicationException("Player not found");
        IList<IMove> allValidMoves = [];

        IPawn[] allPawns = currentPlayer.School.AllPawns;
        IList<IMoveCard> moveCards = currentPlayer.MoveCards;
        Direction direction = currentPlayer.Direction;

        allValidMoves = allPawns.SelectMany(pawn => moveCards.SelectMany(moveCard => PlayMat.GetValidMoves(pawn, moveCard, direction))).ToList();

        return (IReadOnlyList<IMove>)allValidMoves;
    }

    public void MovePawn(Guid playerId, Guid pawnId, string moveCardName, ICoordinate to)
    {
        IPlayer currentPlayer = Players.FirstOrDefault(p => p.Id == playerId);
        IPlayer oppositePlayer = Players.FirstOrDefault(p => p.Id != playerId);

        IPawn pawn = currentPlayer.School.AllPawns.FirstOrDefault(p => p.Id == pawnId);

        if (playerId != PlayerToPlayId)
        {
            throw new ApplicationException("It's not your turn.");
        }

        if (currentPlayer == null)
        {
            throw new ArgumentException("Player not found.");
        }

        if (pawn == null)
        {
            throw new ArgumentException("Pawn not found.");
        }
        IMoveCard moveCard = currentPlayer.MoveCards.FirstOrDefault(mc => mc.Name == moveCardName) ?? throw new ApplicationException("This is not your card");

        IMove chosenMove = new Move(moveCard, pawn, currentPlayer.Direction, to);
        PlayMat.ExecuteMove(chosenMove, out IPawn capturedPawn);       

        if (capturedPawn != null && capturedPawn.Type == PawnType.Master)
        {
            WayOfTheStoneWinner(currentPlayer.Id);
        }

       
        if (pawn.Type == PawnType.Master && pawn.Position.Equals(oppositePlayer.School.TempleArchPosition))
        {
            WayOfTheStreamWinner(currentPlayer.Id);
        }

        ExchangeCards(moveCardName);
        PlayerToPlayId = GetNextOpponent(playerId).Id;
    }

    public void SkipMovementAndExchangeCard(Guid playerId, string moveCardName)
    {
        if (playerId != PlayerToPlayId)
        {
            throw new ApplicationException("its not your turn");
        }

        if (moveCardName == "" || moveCardName == null)
        {
            throw new ApplicationException("You should choose a card to exchange before skipping.");
        }

        IPlayer currentPlayer = Players.FirstOrDefault(p => p.Id == playerId) ?? throw new ApplicationException("Player not found");      
        IMoveCard moveCard = currentPlayer.MoveCards.FirstOrDefault(mv => mv.Name == moveCardName) ?? throw new ApplicationException("Card not found");

        IReadOnlyList<IMove> validMoves = GetAllPossibleMovesFor(playerId);

        if (validMoves.Count != 0)
        {
            throw new ApplicationException("You can't skip. You still have valid move(s)");
        }
        else
        {
            ExchangeCards(moveCardName);
            PlayerToPlayId = GetNextOpponent(playerId).Id;
        }
    }

    public IPlayer GetNextOpponent(Guid playerId)
    {
        return Players.FirstOrDefault(p => playerId != p.Id);
    }

    private void WayOfTheStoneWinner(Guid winner)
    {
        WinnerPlayerId = winner;
        WinnerMethod = "Way Of The Stone";
    }

    private void WayOfTheStreamWinner(Guid winner)
    {
            WinnerPlayerId = winner;
            WinnerMethod = "Way Of The Stream";
    }

    private void ExchangeCards(string moveCardName)
    {
        IPlayer currentPlayer = Players.First(p => p.Id == PlayerToPlayId);
        IMoveCard moveCard;
        int indexOfMoveCard = 0;

        for (int i = 0; i < currentPlayer.MoveCards.Count; i++)
        {
            if (currentPlayer.MoveCards[i].Name == moveCardName)
            {
                moveCard = currentPlayer.MoveCards[i];
                indexOfMoveCard = i;
            }
        }
        (ExtraMoveCard, currentPlayer.MoveCards[indexOfMoveCard]) = (currentPlayer.MoveCards[indexOfMoveCard], ExtraMoveCard);
    }
}
