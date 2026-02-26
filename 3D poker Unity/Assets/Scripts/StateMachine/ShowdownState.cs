using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PokerGame.Core;
using PokerGame.Managers;

namespace PokerGame.StateMachine
{
    public class ShowdownState : IGameState
    {
        private GameManager _gm;
        public ShowdownState(GameManager gm) { _gm = gm; }

        public void Enter()
        {
            var active = _gm.Players.Where(p => !p.IsFolded).ToList();
            
            if (active.Count == 1)
            {
                // Winner by default (others folded)
                int winnerId = active[0].Id;
                var awards = _gm.Chips.DistributePot(_gm.Players, new List<int> { winnerId });
                EventBus.RoundEnded(new[] { winnerId }, awards.Values.Sum());
            }
            else
            {
                // Actual Showdown (multiple players remaining)
                var hands = active.ToDictionary(p => p.Id, p => _gm.Evaluator.Evaluate(p.HoleCards, _gm.CommunityCards.ToArray()));
                
                foreach(var kv in hands) EventBus.HandEvaluated(kv.Key, kv.Value);

                var best = hands.Values.Max();
                var winners = hands.Where(kv => kv.Value.CompareTo(best) == 0).Select(kv => kv.Key).ToList();
                
                var awards = _gm.Chips.DistributePot(_gm.Players, winners);
                EventBus.RoundEnded(winners.ToArray(), awards.Values.Sum());
            }

            _gm.DealerIdx = (_gm.DealerIdx + 1) % _gm.Players.Count;
            _gm.StartCoroutine(WaitAndRestart());
        }

        private IEnumerator WaitAndRestart()
        {
            yield return new WaitForSeconds(3f);
            
            // Check Match Over
            if (_gm.Players.Count(p => p.Chips > 0) <= 1)
                Debug.Log("Match Over!");
            else
                _gm.StateMachine.TransitionTo(GameState.PreFlop);
        }

        public void Execute() { }
        public void Exit() { }
    }
}
