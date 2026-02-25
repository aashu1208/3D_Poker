using System;
using PokerGame.Core;

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
    }

    /// <summary>
    /// Local offline simulation implementation.
    /// </summary>
    public class LocalNetworkAdapter : INetworkAdapter
    {
        public event Action<int, PlayerAction, int> OnActionReceived;

        public void SendPlayerAction(int playerId, PlayerAction action, int amount)
        {
            // Instantly loop back for local play
            OnActionReceived?.Invoke(playerId, action, amount);
        }

        public void BroadcastGameState(GameState state) { } // Handled by EventBus locally
    }
}
