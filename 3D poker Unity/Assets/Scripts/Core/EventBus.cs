using System;
using System.Collections.Generic;

namespace PokerGame.Core
{
    /// <summary>
    /// Static EventBus for completely decoupled communication.
    /// </summary>
    public static class EventBus
    {
        public static event Action<GameState> OnGameStateChanged;
        public static void GameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);

        public static event Action<int, CardData[]> OnHoleCardsDealt;
        public static void HoleCardsDealt(int id, CardData[] c) => OnHoleCardsDealt?.Invoke(id, c);

        public static event Action<CardData[]> OnCommunityCardsRevealed;
        public static void CommunityCardsRevealed(CardData[] c) => OnCommunityCardsRevealed?.Invoke(c);

        public static event Action<int, PlayerAction, int> OnPlayerActionTaken;
        public static void PlayerActionTaken(int id, PlayerAction a, int amt) => OnPlayerActionTaken?.Invoke(id, a, amt);

        public static event Action<int> OnTotalPotUpdated;
        public static void TotalPotUpdated(int pot) => OnTotalPotUpdated?.Invoke(pot);

        public static event Action<int, int> OnChipsChanged; // playerId, newAmount
        public static void ChipsChanged(int id, int amt) => OnChipsChanged?.Invoke(id, amt);

        public static event Action<int> OnTurnChanged;
        public static void TurnChanged(int id) => OnTurnChanged?.Invoke(id);

        public static event Action<int, float> OnTurnTimerTick;
        public static void TurnTimerTick(int id, float remaining) => OnTurnTimerTick?.Invoke(id, remaining);

        public static event Action<int[], int> OnRoundEnded; // winners, pot size
        public static void RoundEnded(int[] w, int pot) => OnRoundEnded?.Invoke(w, pot);

        public static event Action<int, PokerHand> OnHandEvaluated;
        public static void HandEvaluated(int id, PokerHand hand) => OnHandEvaluated?.Invoke(id, hand);

        public static event Action OnGameRestarted;
        public static void GameRestarted() => OnGameRestarted?.Invoke();

        public static void ClearAll()
        {
            OnGameStateChanged = null; OnHoleCardsDealt = null; OnCommunityCardsRevealed = null;
            OnPlayerActionTaken = null; OnTotalPotUpdated = null; OnChipsChanged = null;
            OnTurnChanged = null; OnTurnTimerTick = null; OnRoundEnded = null; OnHandEvaluated = null;
            OnGameRestarted = null;
        }
    }
}
