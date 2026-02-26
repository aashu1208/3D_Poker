using System.Collections;
using System.Linq;
using UnityEngine;
using PokerGame.Core;
using PokerGame.Network;

namespace PokerGame.Managers
{
    /// <summary>
    /// Handles turn passing and 15s timer countdown.
    /// </summary>
    public class TurnTimer : MonoBehaviour
    {
        private GameManager _gm;
        private int _currentIdx;
        private Coroutine _timer;
        private bool _activeRound = false;

        public void Init(GameManager gm) { _gm = gm; }

        public void StartRound(int startIdx)
        {
            StopTimer();
            _activeRound = true;
            _currentIdx = startIdx;
            Advance();
        }

        public void StopTimer()
        {
            if (_timer != null) StopCoroutine(_timer);
            _timer = null;
            _activeRound = false;
        }

        private void Advance()
        {
            if (_timer != null) StopCoroutine(_timer);

            int activeCount = _gm.Players.Count(p => p.CanAct);
            if (activeCount <= 1 || IsBettingDone())
            {
                _activeRound = false;
                NextState();
                return;
            }

            var p = _gm.Players[_currentIdx];
            if (!p.CanAct)
            {
                _currentIdx = (_currentIdx + 1) % _gm.Players.Count;
                Advance();
                return;
            }

            EventBus.TurnChanged(p.Id);
            Debug.Log($"[TurnTimer] Starting turn for Player {p.Id} ({p.Name})");
            _timer = StartCoroutine(TurnRoutine(p));
        }

        private IEnumerator TurnRoutine(PlayerData p)
        {
            float totalTime = p.IsAI ? Random.Range(1.5f, 3f) : 15f;
            float elapsed = 0;

            while (elapsed < totalTime)
            {
                elapsed += Time.deltaTime;
                // Consistent real-time countdown for everyone (15s decreasing normally)
                float displayTime = 15f - elapsed;
                EventBus.TurnTimerTick(p.Id, displayTime);
                yield return null;
            }
            Debug.Log($"[TurnTimer] Time up for Player {p.Id}");

            if (p.IsAI)
            {
                var action = _gm.Evaluator.DecideAIAction(p, _gm.CommunityCards.ToArray(), _gm.Chips.CurrentBet, _gm.Chips.TotalPot, out int curAmt);
                SubmitAction(p.Id, action, curAmt);
            }
            else
            {
                // Timeout action for human
                SubmitAction(p.Id, p.CurrentBet < _gm.Chips.CurrentBet ? PlayerAction.Fold : PlayerAction.Check, 0);
            }
        }

        public void SubmitAction(int id, PlayerAction action, int amt = 0)
        {
            if (!_activeRound) return;
            
            // If human player submits, we stop the 15s timer immediately
            if (_timer != null) StopCoroutine(_timer);
            _timer = null;

            var p = _gm.Players[id];
            _gm.Chips.ProcessAction(p, action, amt);
            
            if (action == PlayerAction.Fold && _gm.Players.Count(x => !x.IsFolded) == 1)
            {
                _activeRound = false;
                _gm.StateMachine.TransitionTo(GameState.Showdown);
                return;
            }

            _currentIdx = (_currentIdx + 1) % _gm.Players.Count;
            Advance();
        }

        // Bridge for UI to call
        public void RequestAction(PlayerAction action, int amt = 0)
        {
            // Architecture: UI calls Network, Network calls Game
            _gm.Network.SendPlayerAction(0, action, amt);
        }

        private bool IsBettingDone()
        {
            var active = _gm.Players.Where(p => !p.IsFolded && !p.IsAllIn).ToList();
            if (active.Count == 0) return true;
            
            // 1. Everyone must have matched the CurrentBet
            bool allMatched = active.All(p => p.CurrentBet == _gm.Chips.CurrentBet);
            
            // 2. Everyone must have had a chance to act in THIS round
            // If someone hasn't acted yet (CurrentBet == 0 AND others haven't bet), we need to let them check.
            // If someone has matched the Big Blind or a Raise, they are done.
            bool everyoneActed = active.All(p => p.CurrentBet > 0 || _gm.Chips.CurrentBet == 0);
            
            // Edge case: Big Blind option to raise. If CurrentBet is 20 and everyone is at 20, 
            // but the BB hasn't "acted" (checked) yet, we continue.
            // However, for this simple engine, we consider the round done if everyone matches.
            
            return allMatched && everyoneActed;
        }

        private void NextState()
        {
            var next = _gm.StateMachine.CurrentState switch {
                GameState.PreFlop => GameState.Flop, GameState.Flop => GameState.Turn,
                GameState.Turn => GameState.River, _ => GameState.Showdown
            };
            _gm.StateMachine.TransitionTo(next);
        }
    }
}
