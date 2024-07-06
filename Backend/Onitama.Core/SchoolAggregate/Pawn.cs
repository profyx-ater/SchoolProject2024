using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.SchoolAggregate;

/// <inheritdoc cref="IPawn"/>
internal class Pawn : IPawn
{
    public Pawn(Guid ownerId, PawnType pawntype)
    {
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Type = pawntype;
        
    }
    
    public Guid Id { get;  }

    public Guid OwnerId { get; }

    public PawnType Type { get; }

    public ICoordinate Position { get; set; }
}