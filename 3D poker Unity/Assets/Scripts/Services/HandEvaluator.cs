using System;
using System.Collections.Generic;
using System.Linq;
using PokerGame.Core;

namespace PokerGame.Services
{
    /// <summary>
    /// Pure C# Hand Evaluator AND AI Decision Maker.
    /// Evaluates best 5-card hands and provides AI action decisions based on strength.
    /// </summary>
    public class HandEvaluator
    {
        private readonly System.Random _rng = new System.Random();

        // ── Public AI Logic ───────────────────────────────────────────────

        public PlayerAction DecideAIAction(PlayerData ai, CardData[] community, int currentBet, int pot, out int raiseAmt)
        {
            raiseAmt = 0;
            float strength = GetStrength(ai, community);
            float odds = CalculatePotOdds(currentBet, pot, ai.CurrentBet);
            int toCall = currentBet - ai.CurrentBet;
            bool canCheck = toCall == 0;

            // Simple rules
            if (strength > 0.65f || (_rng.NextDouble() < 0.1f && !canCheck)) // Bluff 10%
            {
                raiseAmt = Math.Min(currentBet * 2 + 20, ai.Chips);
                return PlayerAction.Raise;
            }
            if (strength >= odds || strength > 0.35f)
            {
                if (canCheck) return PlayerAction.Check;
                if (toCall >= ai.Chips) { raiseAmt = ai.Chips; return PlayerAction.AllIn; }
                return PlayerAction.Call;
            }
            return canCheck ? PlayerAction.Check : PlayerAction.Fold;
        }

        private float GetStrength(PlayerData p, CardData[] comm)
        {
            if (comm == null || comm.Length == 0) return PreFlopStrength(p.HoleCards);
            return Evaluate(p.HoleCards, comm).StrengthScore;
        }

        private float CalculatePotOdds(int bet, int pot, int curBet)
        {
            int toCall = bet - curBet;
            if (toCall <= 0 || pot + toCall == 0) return 0;
            return (float)toCall / (pot + toCall);
        }

        private float PreFlopStrength(CardData[] h)
        {
            if (h == null || h.Length < 2) return 0;
            float score = 0;
            if (h[0].Rank == h[1].Rank) score = 0.5f + ((int)h[0].Rank - 2) / 12f * 0.4f; // Pair
            float high = Math.Max((int)h[0].Rank, (int)h[1].Rank);
            score = Math.Max(score, (high - 2) / 12f * 0.45f);
            if (h[0].Suit == h[1].Suit) score += 0.05f; // Suited
            return Math.Min(1f, score);
        }

        // ── Public Evaluation Logic ────────────────────────────────────

        public PokerHand Evaluate(CardData[] holeCards, CardData[] communityCards)
        {
            var cards = holeCards.Concat(communityCards).ToArray();
            
            if (cards.Length < 5)
            {
                // Fallback for fewer than 5 cards: basic rank evaluation
                return Eval5(cards); 
            }

            PokerHand best = null;
            foreach (var combo in Combinations(cards, 5))
            {
                var h = Eval5(combo);
                if (best == null || h.CompareTo(best) > 0) best = h;
            }
            return best;
        }

        // ── Internal Eval ──────────────────────────────────────────────

        private PokerHand Eval5(CardData[] c)
        {
            if (c.Length == 0) return new PokerHand(HandRank.HighCard, c, new int[0]);
            
            Array.Sort(c, (a, b) => ((int)b.Rank).CompareTo((int)a.Rank));
            bool isFlush = c.Length >= 5 && c.All(x => x.Suit == c[0].Suit);
            bool isStraight = IsStraight(c, out int hr);
            var g = c.GroupBy(x => x.Rank).Select(x => x.ToList()).OrderByDescending(x => x.Count).ThenByDescending(x => (int)x[0].Rank).ToList();

            if (isFlush && isStraight) return new PokerHand(hr == 14 ? HandRank.RoyalFlush : HandRank.StraightFlush, c, new[] { hr });
            if (g.Any(x => x.Count == 4)) return new PokerHand(HandRank.FourOfAKind, c, new[] { (int)g[0][0].Rank, (int)g[1][0].Rank });
            if (g.Any(x => x.Count == 3) && g.Any(x => x.Count == 2)) return new PokerHand(HandRank.FullHouse, c, new[] { (int)g[0][0].Rank, (int)g[1][0].Rank });
            if (isFlush) return new PokerHand(HandRank.Flush, c, c.Select(x => (int)x.Rank).ToArray());
            if (isStraight) return new PokerHand(HandRank.Straight, c, new[] { hr });
            if (g.Any(x => x.Count == 3)) 
            {
                var kickers = g.Skip(1).Select(x => (int)x[0].Rank).ToArray();
                return new PokerHand(HandRank.ThreeOfAKind, c, new[] { (int)g[0][0].Rank }.Concat(kickers).ToArray());
            }
            if (g.Count(x => x.Count == 2) >= 2) 
            {
                var kickers = g.Skip(2).Take(1).Select(x => (int)x[0].Rank).ToArray();
                return new PokerHand(HandRank.TwoPair, c, new[] { (int)g[0][0].Rank, (int)g[1][0].Rank }.Concat(kickers).ToArray());
            }
            if (g.Any(x => x.Count == 2)) 
            {
                var kickers = g.Skip(1).Select(x => (int)x[0].Rank).ToArray();
                return new PokerHand(HandRank.OnePair, c, new[] { (int)g[0][0].Rank }.Concat(kickers).ToArray());
            }
            
            return new PokerHand(HandRank.HighCard, c, c.Select(x => (int)x.Rank).ToArray());
        }

        private bool IsStraight(CardData[] c, out int high)
        {
            var r = c.Select(x => (int)x.Rank).Distinct().OrderByDescending(x => x).ToList();
            high = r.Count > 0 ? r[0] : 0;
            if (r.Count < 5) return false;
            if (r[0] - r[4] == 4) return true;
            if (r[0] == 14 && r[1] == 5 && r[2] == 4 && r[3] == 3 && r[4] == 2) { high = 5; return true; }
            return false;
        }

        private IEnumerable<CardData[]> Combinations(CardData[] c, int choose)
        {
            int n = c.Length; 
            if (n < choose) yield break;
            
            int[] idx = Enumerable.Range(0, choose).ToArray();
            while (true)
            {
                yield return idx.Select(i => c[i]).ToArray();
                int i2 = choose - 1;
                while (i2 >= 0 && idx[i2] == i2 + n - choose) i2--;
                if (i2 < 0) yield break;
                idx[i2]++;
                for (int j = i2 + 1; j < choose; j++) idx[j] = idx[j - 1] + 1;
            }
        }
    }
}
