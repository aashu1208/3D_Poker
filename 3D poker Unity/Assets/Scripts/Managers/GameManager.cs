using System.Collections.Generic;
using UnityEngine;
using PokerGame.Core;
using PokerGame.Services;
using PokerGame.StateMachine;
using PokerGame.Network;

namespace PokerGame.Managers
{
    /// <summary>
    /// Bootstrapper and main logic holder. 
    /// No heavy game logic inside, just wires things together.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public int PlayerCount = 4;
        public int StartingChips = 1000;
        public int SmallBlindAmt = 10;
        public int BigBlindAmt = 20;

        [Header("References")]
        public TurnTimer TurnTimer;

        // Core references
        public GameStateController StateMachine { get; private set; }
        public DeckService Deck { get; private set; }
        public HandEvaluator Evaluator { get; private set; }
        public ChipManagerService Chips { get; private set; }
        public INetworkAdapter Network { get; private set; }

        // State
        public List<PlayerData> Players { get; private set; } = new List<PlayerData>();
        public List<CardData> CommunityCards { get; private set; } = new List<CardData>();
        public int DealerIdx = 0;
        public int SmallBlindIdx => (DealerIdx + 1) % Players.Count;
        public int BigBlindIdx => (DealerIdx + 2) % Players.Count;

        private void Awake()
        {
            Deck = new DeckService();
            Evaluator = new HandEvaluator();
            Chips = new ChipManagerService();
            Network = new LocalNetworkAdapter();

            for (int i = 0; i < PlayerCount; i++)
                Players.Add(new PlayerData(i, i == 0 ? "You" : $"AI {i}", StartingChips, i != 0));

            TurnTimer.Init(this);

            StateMachine = new GameStateController();
            StateMachine.RegisterState(GameState.Idle, new IdleState()); // Empty state
            StateMachine.RegisterState(GameState.PreFlop, new PreFlopState(this));
            StateMachine.RegisterState(GameState.Flop, new FlopState(this));
            StateMachine.RegisterState(GameState.Turn, new TurnState(this));
            StateMachine.RegisterState(GameState.River, new RiverState(this));
            StateMachine.RegisterState(GameState.Showdown, new ShowdownState(this));
        }

        private void Start()
        {
            StateMachine.TransitionTo(GameState.PreFlop);
        }

        private void Update() => StateMachine.Tick();

        public void RestartMatch()
        {
            StopAllCoroutines();
            TurnTimer.StopTimer();
            
            DealerIdx = 0;
            CommunityCards.Clear();
            Chips.ResetRound();
            
            foreach (var p in Players)
            {
                p.Chips = StartingChips;
                p.ResetForNewRound();
            }
            
            EventBus.GameRestarted();
            StateMachine.TransitionTo(GameState.PreFlop);
        }

        // Idle state wrapper
        private class IdleState : IGameState { public void Enter(){} public void Execute(){} public void Exit(){} }
    }
}
