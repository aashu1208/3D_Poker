using PokerGame.Core;
using PokerGame.Managers;

namespace PokerGame.StateMachine
{
    public class RiverState : IGameState
    {
        private GameManager _gm;
        public RiverState(GameManager gm) { _gm = gm; }

        public void Enter()
        {
            _gm.Chips.ResetBettingRound();
            var card = new[] { _gm.Deck.DealCard() };
            _gm.CommunityCards.AddRange(card);
            EventBus.CommunityCardsRevealed(card);
            
            _gm.TurnTimer.StartRound(_gm.SmallBlindIdx);
        }

        public void Execute() { }
        public void Exit() { foreach (var p in _gm.Players) p.CurrentBet = 0; }
    }
}
