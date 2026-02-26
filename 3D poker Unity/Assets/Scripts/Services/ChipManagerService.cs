using System;
using System.Collections.Generic;
using System.Linq;
using PokerGame.Core;

namespace PokerGame.Services
{
    /// <summary>
    /// Pure C# Chip Manager handling bets, raises, side-pots, and pot distribution.
    /// Publishes results to EventBus.
    /// </summary>
    public class ChipManagerService
    {
        public int TotalPot { get; private set; }
        public int CurrentBet { get; private set; }

        public void ResetRound() { TotalPot = 0; CurrentBet = 0; }
        
        public void ResetBettingRound() { CurrentBet = 0; }
        
        public void SetCurrentBet(int amt) => CurrentBet = amt;

        public void ProcessAction(PlayerData p, PlayerAction a, int amt = 0)
        {
            switch (a)
            {
                case PlayerAction.Call:
                    int callAmt = Math.Min(CurrentBet - p.CurrentBet, p.Chips);
                    AddBet(p, callAmt);
                    if (p.Chips == 0) p.IsAllIn = true;
                    break;
                case PlayerAction.Raise:
                    int toAdd = Math.Min(amt - p.CurrentBet, p.Chips);
                    AddBet(p, toAdd);
                    CurrentBet = p.CurrentBet;
                    if (p.Chips == 0) p.IsAllIn = true;
                    break;
                case PlayerAction.AllIn:
                    AddBet(p, p.Chips);
                    if (p.CurrentBet > CurrentBet) CurrentBet = p.CurrentBet;
                    p.IsAllIn = true;
                    break;
                case PlayerAction.Fold:
                    p.IsFolded = true;
                    break;
            }
            EventBus.PlayerActionTaken(p.Id, a, amt);
        }

        private void AddBet(PlayerData p, int amt)
        {
            if (amt <= 0) return;
            p.Chips -= amt;
            p.CurrentBet += amt;
            p.TotalBetRound += amt;
            TotalPot += amt;

            EventBus.ChipsChanged(p.Id, p.Chips);
            EventBus.TotalPotUpdated(TotalPot);
        }

        public Dictionary<int, int> DistributePot(List<PlayerData> activePlayers, List<int> winners)
        {
            var awards = activePlayers.ToDictionary(p => p.Id, p => 0);
            
            // Simplified Distribution (Handles side pots conceptually based on total bets)
            var pots = activePlayers.Where(p => p.TotalBetRound > 0).Select(p => p.TotalBetRound).Distinct().OrderBy(c => c).ToList();
            int prev = 0;

            foreach (int cap in pots)
            {
                int level = cap - prev;
                int potAmt = activePlayers.Sum(p => Math.Min(p.TotalBetRound - prev, level));
                if (potAmt <= 0) { prev = cap; continue; }

                var eligible = activePlayers.Where(p => p.TotalBetRound >= cap && !p.IsFolded).Select(p => p.Id).ToList();
                var validWinners = winners.Where(w => eligible.Contains(w)).ToList();
                if (validWinners.Count == 0) validWinners = eligible.Take(1).ToList(); // Fallback

                int share = potAmt / validWinners.Count;
                int rem = potAmt - share * validWinners.Count;

                foreach (int w in validWinners) awards[w] += share;
                if (validWinners.Count > 0) awards[validWinners[0]] += rem;

                prev = cap;
            }

            foreach (var kvp in awards)
            {
                if (kvp.Value > 0)
                {
                    var p = activePlayers.First(x => x.Id == kvp.Key);
                    p.Chips += kvp.Value;
                    EventBus.ChipsChanged(p.Id, p.Chips);
                }
            }

            TotalPot = 0;
            return awards;
        }
    }
}
