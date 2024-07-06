using Onitama.Core.GameAggregate;
using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.PlayMatAggregate.Contracts;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.TableAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.PlayMatAggregate;

/// <inheritdoc cref="IPlayMat"/>
internal class PlayMat : IPlayMat
{
    /// <summary>
    /// Creates a play mat that is a copy of another play mat
    /// </summary>
    /// <param name="otherPlayMat">The play mat steps copy</param>
    /// <param name="copiedPlayers">
    /// Copies of the players (with their school)
    /// that should be used steps position pawn on the copy of the <paramref name="otherPlayMat"/>.</param>
    /// <remarks>
    /// This is an EXTRA. Not needed steps implement the minimal requirements.
    /// To make the mini-max algorithm for an AI game play strategy work, this constructor should be implemented.
    /// </remarks>
    private readonly int _size;
    private readonly IPawn[,] _grid;
    public PlayMat(IPlayMat otherPlayMat, IPlayer[] copiedPlayers)
    {
        throw new NotImplementedException("TODO: copy properties of other playmat");
    }

    public PlayMat(ITable table)
    {
        _size = table.Preferences.PlayerMatSize;
        _grid = new IPawn[_size, _size];
    }

    public IPawn[,] Grid => _grid;

    public int Size => _size;

    public void PositionSchoolOfPlayer(IPlayer player)
    {

        int row = player.Direction == Direction.North ? 0 : Size - 1;

        for (int i = 0; i < Size; i++)
        {
            ICoordinate coordinate = new Coordinate(row, i);
            player.School.AllPawns[i].Position = coordinate;
            Grid[row, i] = player.School.AllPawns[i];
        }

        player.School.TempleArchPosition = new Coordinate(player.School.Master.Position.Row, player.School.Master.Position.Column);
    }

    public IReadOnlyList<IMove> GetValidMoves(IPawn pawn, IMoveCard card, Direction playerDirection)
    {
        List<IMove> possibleMoves = [];
        List<IPawn> pawnsInPossibleMoves = new List<IPawn>();
        ICoordinate pawnPosition = new Coordinate(2, 2); // postion of pawn in the card
        IReadOnlyList<ICoordinate> possibleTargetCoordinates = card.GetPossibleTargetCoordinates(pawn.Position, playerDirection, Size);

        foreach (ICoordinate coordinate in possibleTargetCoordinates)
        {
            IMove move = new Move(card, pawn, playerDirection, coordinate);
            possibleMoves.Add(move);
        }

        pawnsInPossibleMoves = possibleTargetCoordinates.Select(move => Grid[move.Row, move.Column]).ToList();

        List<IMove> validMoves = new List<IMove>();
        int j = 0;

        for (int i = 0; i < possibleMoves.Count; i++)
        {
            IPawn targetPawn = pawnsInPossibleMoves[j];
            if (targetPawn == null || targetPawn.OwnerId != pawn.OwnerId && targetPawn.Id != pawn.Id)
            {
                validMoves.Add(possibleMoves[i]);
            }
            j++;
        }
        return validMoves;
    }

    public void ExecuteMove(IMove move, out IPawn capturedPawn)
    {
        IReadOnlyList<IMove> validMoves = GetValidMoves(move.Pawn, move.Card, move.PlayerDirection);
        if (validMoves.Any(mv => mv.To.Equals(move.To)))
        {
            Grid[move.Pawn.Position.Row, move.Pawn.Position.Column] = null;

            if (Grid[move.To.Row, move.To.Column] != null)
            {
                capturedPawn = Grid[move.To.Row, move.To.Column];
            }
            else
            {
                capturedPawn = null;
            }

            move.Pawn.Position = move.To;
            Grid[move.Pawn.Position.Row, move.Pawn.Position.Column] = move.Pawn;

        }
        else
        {
            throw new ApplicationException("Invalid move");
        }

    }
}
