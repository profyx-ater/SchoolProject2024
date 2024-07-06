using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.TableAggregate;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;
using System.Data.Common;
using System.Drawing;

namespace Onitama.Core.SchoolAggregate;

/// <inheritdoc cref="ISchool"/>
internal class School : ISchool
{
    /// <summary>
    /// Creates a school that is a copy of another school.
    /// </summary>
    /// <remarks>
    /// This is an EXTRA. Not needed to implement the minimal requirements.
    /// To make the mini-max algorithm for an AI game play strategy work, this constructor should be implemented.
    /// </remarks>
    public School(ISchool otherSchool)
    {
        throw new NotImplementedException("TODO: copy properties of other school. Make sure to copy the pawns, not just reference them");
    }


    public School(Guid userId)
    {
        CreatePawns(userId);
    }

    private void CreatePawns(Guid userId)
    {
        AllPawns = new Pawn[5];
        Master = new Pawn(userId, PawnType.Master);
        Students = new Pawn[4];

        for (int i = 0; i < Students.Length; i++)
        {
            Students[i] = new Pawn(userId, PawnType.Student);
        }
        AllPawns[0] = Master;
        for (int i = 1; i < AllPawns.Length; i++)
        {
            AllPawns[i] = Students[i - 1];
        }
        (AllPawns[0],AllPawns[2]) = (AllPawns[2], AllPawns[0]);
    }
    public IPawn Master { get; set; }

    public IPawn[] Students { get; set; }

    public IPawn[] AllPawns { get; set; }

    public ICoordinate TempleArchPosition { get; set; }

    public IPawn GetPawn(Guid pawnId)
    {
        for (int i = 0;i < AllPawns.Length; i++)
        {
            if (AllPawns[i].Id == pawnId)
            {
                return AllPawns[i];
            }
        }

        return null;

    }
}