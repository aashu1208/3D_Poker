using System;
using PokerGame.Core;
using PokerGame.Managers;

namespace PokerGame.Network
{
    /// <summary>
    /// Single interface for network abstraction (Photon ready).
    /// </summary>
    public interface INetworkAdapter
    {
        void SendPlayerAction(int playerId, PlayerAction action, int amount);
        void BroadcastGameState(GameState state);
        event Action<int, PlayerAction, int> OnActionReceived;
        void Init(TurnTimer timer);
    }

    /// <summary>
    /// Local offline simulation implementation.
    /// </summary>
    public class LocalNetworkAdapter : INetworkAdapter
    {
        public event Action<int, PlayerAction, int> OnActionReceived;
        private TurnTimer _timer;

        public void Init(TurnTimer timer) => _timer = timer;

        public void SendPlayerAction(int playerId, PlayerAction action, int amount)
        {
            // Simulation: Broadcast locally
            OnActionReceived?.Invoke(playerId, action, amount);
            // Submit to game state
            _timer?.SubmitAction(playerId, action, amount);
        }

        public void BroadcastGameState(GameState state) { } 
    }
}
