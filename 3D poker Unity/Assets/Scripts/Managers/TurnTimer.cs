using System.Collections;
using System.Linq;
using UnityEngine;
using PokerGame.Core;

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
            _activeRound = true;
            _currentIdx = startIdx;
            Advance();
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
            if (p.IsAI) StartCoroutine(AITurn(p));
            else _timer = StartCoroutine(Countdown(p));
        }

        private IEnumerator Countdown(PlayerData p)
        {
            float elapsed = 0;
            while (elapsed < 15f)
            {
                elapsed += Time.deltaTime;
                EventBus.TurnTimerTick(p.Id, 15f - elapsed);
                yield return null;
            }
            SubmitAction(p.Id, p.CurrentBet < _gm.Chips.CurrentBet ? PlayerAction.Fold : PlayerAction.Check, 0);
        }

        private IEnumerator AITurn(PlayerData p)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2.5f));
            var action = _gm.Evaluator.DecideAIAction(p, _gm.CommunityCards.ToArray(), _gm.Chips.CurrentBet, _gm.Chips.TotalPot, out int curAmt);
            SubmitAction(p.Id, action, curAmt);
        }

        public void SubmitAction(int id, PlayerAction action, int amt = 0)
        {
            if (!_activeRound) return;
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

        private bool IsBettingDone()
        {
            var active = _gm.Players.Where(p => !p.IsFolded && !p.IsAllIn).ToList();
            if (active.Count == 0) return true;
            bool allMatched = active.All(p => p.CurrentBet == _gm.Chips.CurrentBet);
            bool allActed = active.All(p => p.CurrentBet > 0 || _gm.Chips.CurrentBet == 0 || p.TotalBetRound > 0);
            return allMatched && allActed;
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
