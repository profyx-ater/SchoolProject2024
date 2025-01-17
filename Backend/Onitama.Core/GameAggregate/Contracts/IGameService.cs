﻿using Onitama.Core.SchoolAggregate.Contracts;
using Onitama.Core.UserAggregate;
using Onitama.Core.Util.Contracts;

namespace Onitama.Core.GameAggregate.Contracts;

public interface IGameService
{
    IGame GetGame(Guid gameId);
    IReadOnlyList<IMove> GetPossibleMovesForPawn(Guid gameId, Guid playerId, Guid pawnId, string moveCardName);
    IReadOnlyList<IMove> GetAllPossibleMovesFromOpponent(Guid gameId, Guid playerId);
    void MovePawn(Guid gameId, Guid playerId, Guid pawnId, string moveCardName, ICoordinate to);
    void SkipMovementAndExchangeCard(Guid gameId, Guid playerId, string moveCardName);
}