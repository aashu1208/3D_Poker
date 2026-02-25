using System.Collections.Generic;
using System.Linq;
using PokerGame.Core;
using PokerGame.Services;
using PokerGame.Managers;

namespace PokerGame.StateMachine
{
    /// <summary>
    /// Resets deck, deals 2 hole cards, posts blinds, and hands control to TurnTimer.
    /// </summary>
    public class PreFlopState : IGameState
    {
        private GameManager _gm;
        
        public PreFlopState(GameManager gm) { _gm = gm; }

        public void Enter()
        {
            _gm.CommunityCards.Clear();
            _gm.Chips.ResetRound();
            _gm.Deck.Initialize();

            // Deal & Blinds
            var active = _gm.Players.Where(p => p.Chips > 0).ToList();
            foreach (var p in active)
            {
                p.ResetForNewRound();
                p.HoleCards = _gm.Deck.DealCards(2);
                EventBus.HoleCardsDealt(p.Id, p.HoleCards);
            }

            if (active.Count >= 2)
            {
                _gm.Chips.ProcessAction(active[_gm.SmallBlindIdx], PlayerAction.Raise, _gm.SmallBlindAmt); // Small Blind
                _gm.Chips.ProcessAction(active[_gm.BigBlindIdx], PlayerAction.Raise, _gm.BigBlindAmt); // Big Blind
            }

            int utg = (_gm.BigBlindIdx + 1) % active.Count;
            _gm.TurnTimer.StartRound(utg);
        }

        public void Execute() { }
        public void Exit() { foreach (var p in _gm.Players) p.CurrentBet = 0; }
    }
}
