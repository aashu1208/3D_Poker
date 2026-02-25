using System;
using System.Collections.Generic;

namespace PokerGame.Core
{
    // ── Enums ───────────────────────────────────────────────────────────────

    public enum GameState { Idle, PreFlop, Flop, Turn, River, Showdown }

    public enum PlayerAction { None, Fold, Check, Call, Raise, AllIn }

    public enum HandRank
    {
        HighCard = 0, OnePair = 1, TwoPair = 2, ThreeOfAKind = 3, Straight = 4,
        Flush = 5, FullHouse = 6, FourOfAKind = 7, StraightFlush = 8, RoyalFlush = 9
    }

    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public enum Rank { Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14 }

    // ── Structs & Models ────────────────────────────────────────────────────

    [Serializable]
    public struct CardData
    {
        public Suit Suit;
        public Rank Rank;

        public CardData(Suit suit, Rank rank) { Suit = suit; Rank = rank; }
        public override string ToString() => $"{Rank} of {Suit}";
    }

    [Serializable]
    public class PlayerData
    {
        public int Id;
        public string Name;
        public int Chips;
        public int CurrentBet;
        public int TotalBetRound;    // Used for side-pot logic
        public bool IsAI;
        public bool IsFolded;
        public bool IsAllIn;
        public CardData[] HoleCards = new CardData[2];

        public bool CanAct => !IsFolded && !IsAllIn && Chips > 0;

        public PlayerData(int id, string name, int chips, bool isAI)
        {
            Id = id; Name = name; Chips = chips; IsAI = isAI;
        }

        public void ResetForNewRound()
        {
            CurrentBet = 0; TotalBetRound = 0; IsFolded = false; IsAllIn = (Chips == 0);
            HoleCards = new CardData[2];
        }
    }

    public class PokerHand : IComparable<PokerHand>
    {
        public HandRank Rank;
        public CardData[] BestCards;
        public int[] Kickers;

        public float StrengthScore
        {
            get
            {
                float baseScore = (float)Rank / 9f * 0.9f;
                float kickerScore = Kickers != null && Kickers.Length > 0 ? (Kickers[0] - 2f) / 12f * 0.1f : 0f;
                return UnityEngine.Mathf.Clamp01(baseScore + kickerScore);
            }
        }

        public PokerHand(HandRank rank, CardData[] cards, int[] kickers)
        {
            Rank = rank; BestCards = cards; Kickers = kickers;
        }

        public int CompareTo(PokerHand other)
        {
            if (other == null) return 1;
            int rankDiff = Rank.CompareTo(other.Rank);
            if (rankDiff != 0) return rankDiff;

            int len = Math.Min(Kickers.Length, other.Kickers.Length);
            for (int i = 0; i < len; i++)
            {
                int diff = Kickers[i].CompareTo(other.Kickers[i]);
                if (diff != 0) return diff;
            }
            return 0; // Absolute tie
        }
    }
}
