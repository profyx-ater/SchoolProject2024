using System.Collections.Generic;
using System.Drawing;
using Onitama.Core.GameAggregate;
using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.SchoolAggregate;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.MoveCardAggregate;

/// <inheritdoc cref="IMoveCard"/>
internal class MoveCard : IMoveCard
{
    public string Name { get; }

    public MoveCardGridCellType[,] Grid { get; }

    public Color StampColor { get; }

    public MoveCard(string name, MoveCardGridCellType[,] grid, Color stampColor)
    {
        Name = name;
        Grid = grid;
        StampColor = stampColor;
    }

    //Do not change this method, it makes sure that two MoveCard instances are equal if their names are equal
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return obj is IMoveCard other && Equals(other);
    }

    //Do not change this method
    protected bool Equals(IMoveCard other)
    {
        return Name == other.Name;
    }

    //Do not change this method
    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }

    public IReadOnlyList<ICoordinate> GetPossibleTargetCoordinates(ICoordinate startCoordinate, Direction playDirection, int matSize)
    {
        ICoordinate pawnPosition = new Coordinate(2, 2); // postion of pawn in the card
        List<ICoordinate> possibleTargetCoordinates = [];
        int startRow, endRow, startCol, endCol, rowStep, colStep;


        if (playDirection == Direction.North)
        {
            startRow = 0;
            endRow = matSize;
            startCol = 0;
            endCol = matSize;
            rowStep = 1;
            colStep = 1;
        }
        else
        {
            startRow = matSize - 1;
            endRow = -1;
            startCol = matSize - 1;
            endCol = -1;
            rowStep = -1;
            colStep = -1;
        }

        for (int row = startRow; row != endRow; row += rowStep)
        {
            for (int col = startCol; col != endCol; col += colStep)
            {
                if (Grid[row, col] == MoveCardGridCellType.Target)
                {
                    int numberOfRows = row - pawnPosition.Row;
                    int numberOfColumns = col - pawnPosition.Column;
                    ICoordinate steps = new Coordinate(numberOfRows, numberOfColumns).RotateTowards(playDirection);
                    Coordinate coordinate = new Coordinate(steps.Row + startCoordinate.Row, steps.Column + startCoordinate.Column);
                    if (!coordinate.IsOutOfBounds(matSize))
                    {
                        possibleTargetCoordinates.Add(coordinate);
                    }
                }
            }
        }
        return possibleTargetCoordinates;
    }
}