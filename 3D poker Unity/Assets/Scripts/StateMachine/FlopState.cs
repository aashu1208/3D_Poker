using System.Linq;
using PokerGame.Core;
using PokerGame.Managers;

namespace PokerGame.StateMachine
{
    public class FlopState : IGameState
    {
        private GameManager _gm;
        public FlopState(GameManager gm) { _gm = gm; }

        public void Enter()
        {
            var cards = _gm.Deck.DealCards(3);
            _gm.CommunityCards.AddRange(cards);
            EventBus.CommunityCardsRevealed(cards);
            
            _gm.TurnTimer.StartRound(_gm.SmallBlindIdx);
        }

        public void Execute() { }
        public void Exit() { foreach (var p in _gm.Players) p.CurrentBet = 0; }
    }
}
