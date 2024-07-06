using System.Drawing;
using Onitama.Core.MoveCardAggregate.Contracts;

namespace Onitama.Core.MoveCardAggregate;

/// <inheritdoc cref="IMoveCardFactory"/>
internal class MoveCardFactory : IMoveCardFactory
{
    public IMoveCard Create(string name, MoveCardGridCellType[,] grid, Color[] possibleStampColors)
    {
        Random random = new Random();
        int index = random.Next(0, possibleStampColors.Length);
        Color color = possibleStampColors[index];
        IMoveCard moveCard = new MoveCard(name, grid, color);

        return moveCard;
    }
}