using System;
using System.Collections.Generic;
using PokerGame.Core;

namespace PokerGame.StateMachine
{
    public interface IGameState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    /// <summary>
    /// Core brain driving poker round flow. 
    /// Calls Enter/Exit on 5 main states.
    /// </summary>
    public class GameStateController
    {
        private readonly Dictionary<GameState, IGameState> _states = new Dictionary<GameState, IGameState>();
        public GameState CurrentState { get; private set; } = GameState.Idle;
        private IGameState _activeState;

        public void RegisterState(GameState key, IGameState state) => _states[key] = state;

        public void TransitionTo(GameState next)
        {
            if (!_states.ContainsKey(next)) throw new InvalidOperationException($"No state for {next}");
            _activeState?.Exit();
            CurrentState = next;
            _activeState = _states[next];
            EventBus.GameStateChanged(CurrentState);
            _activeState.Enter();
        }

        public void Tick() => _activeState?.Execute();
    }
}
