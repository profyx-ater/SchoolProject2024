using System.Numerics;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.Util;

/// <inheritdoc cref="ICoordinate"/>
internal class Coordinate : ICoordinate
{
    public int Row { get; }
    public int Column { get; }

    public Coordinate(int row, int column)
    {
        Row = row;
        Column = column;
    }

    //Do not change this method
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return obj is ICoordinate other && Equals(other);
    }

    //Do not change this method
    protected bool Equals(ICoordinate other)
    {
        return Row == other.Row && Column == other.Column;
    }

    //Do not change this method
    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }

    //Do not change this method
    public override string ToString()
    {
        return $"({Row}, {Column})";
    }

    public bool IsOutOfBounds(int playMatSize)
    {
        return (Row >= playMatSize || Row < 0) || (Column >= playMatSize || Column < 0);
    }

    public ICoordinate GetNeighbor(Direction direction)
    {
        throw new NotImplementedException();
    }

    public ICoordinate RotateTowards(Direction direction)
    {
        ICoordinate coordinate;
        if (direction == Direction.North)
        {
            coordinate = new Coordinate(Row, Column);
        }
        else if (direction == Direction.South)
        {
            coordinate = new Coordinate(-Row, -Column);
        }else if(direction == Direction.East)
        {
            coordinate = new Coordinate(-Column, Row);
        }
        else
        {
            coordinate = new Coordinate(Column, -Row);
        }
        return coordinate;
    }

    public int GetDistanceTo(ICoordinate other)
    {
        return Row != other.Row ? Math.Abs(Row - other.Row) : Math.Abs(Column - other.Column);
    }
}