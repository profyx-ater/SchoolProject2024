using Onitama.Core.GameAggregate.Contracts;
using Onitama.Core.MoveCardAggregate.Contracts;
using Onitama.Core.PlayerAggregate.Contracts;
using Onitama.Core.PlayMatAggregate;
using Onitama.Core.PlayMatAggregate.Contracts;
using Onitama.Core.TableAggregate.Contracts;
using Onitama.Core.Util;
using Onitama.Core.Util.Contracts;
using System.Drawing;

namespace Onitama.Core.GameAggregate;

internal class GameFactory : IGameFactory
{
    private IMoveCardRepository _moveCardRepository;
    public GameFactory(IMoveCardRepository moveCardRepository)
    {
        _moveCardRepository = moveCardRepository;
        // IGameRepository setten
    }

    public IGame CreateNewForTable(ITable table)
    {
        IPlayMat playmat = new PlayMat(table);

        IMoveCard extraCard = SelectCardForPlayer(table);

        IGame game = new Game(Guid.NewGuid(), playmat, table.SeatedPlayers.ToArray(), extraCard);

        return game;
    }

    private IMoveCard SelectCardForPlayer(ITable table)
    {
        Color[] colors = new Color[table.SeatedPlayers.Count];
        for (int i = 0; i < table.SeatedPlayers.Count; i++)
        {
            colors[i] = table.SeatedPlayers[i].Color;

        }

        IMoveCard[] cardSet = _moveCardRepository.LoadSet(table.Preferences.MoveCardSet, colors);

        Random random = new Random();

        IMoveCard[] usedCards = new IMoveCard[5];

        for (int i = 0; i < usedCards.Length; i++)
        {
            int index = random.Next(0, cardSet.Length);
            usedCards[i] = cardSet[index];
            while (usedCards.Contains(cardSet[index]))
            {
                index = random.Next(0, cardSet.Length);
            }
            usedCards[i] = cardSet[index];
            
        }

        table.SeatedPlayers[0].MoveCards.Add(usedCards[0]);
        table.SeatedPlayers[0].MoveCards.Add(usedCards[1]);
        table.SeatedPlayers[1].MoveCards.Add(usedCards[2]);
        table.SeatedPlayers[1].MoveCards.Add(usedCards[3]);
        IMoveCard extraCard = usedCards[4];

        return extraCard;
    }

    

}
