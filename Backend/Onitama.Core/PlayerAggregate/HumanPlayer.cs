using System.Diagnostics;
using System.Drawing;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.SchoolAggregate;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.UserAggregate;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.PlayerAggregate;

/// <inheritdoc cref="IPlayer"/>
internal class HumanPlayer : PlayerBase, IPlayer, ISchool
{


    public HumanPlayer(Guid userId, string name, Color color, Direction direction) : base(userId, name, color, direction)
    {
   
    }

 

    public IPawn Master { get; set; }

    public IPawn[] Students { get; set; }

    public IPawn[] AllPawns { get; set; }

    public ICoordinate TempleArchPosition { get; set; }

    public IPawn GetPawn(Guid pawnId)
    {
        return AllPawns.FirstOrDefault(pawn => pawn.Id == pawnId);
    }
}