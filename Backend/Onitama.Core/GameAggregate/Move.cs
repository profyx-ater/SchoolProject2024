using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.GameAggregate;

internal class Move : IMove
{
    public IMoveCard Card { get; set; }

    public IPawn Pawn { get; set; }

    public Direction PlayerDirection { get; set; }

    public ICoordinate To { get; set; }

    public Move(IMoveCard card)
    {
    }

    public Move(IMoveCard card, IPawn pawn, Direction playerDirection, ICoordinate to)
    {
        Card = card;
        Pawn = pawn;
        PlayerDirection = playerDirection;
        To = to;
    }
}