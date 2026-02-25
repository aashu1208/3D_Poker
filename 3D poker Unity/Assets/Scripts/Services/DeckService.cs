using System;
using System.Collections.Generic;
using PokerGame.Core;

namespace PokerGame.Services
{
    /// <summary>
    /// Pure C# Deck manager.
    /// </summary>
    public class DeckService
    {
        private readonly List<CardData> _deck = new List<CardData>(52);
        private readonly Random _rng = new Random();

        public void Initialize()
        {
            _deck.Clear();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    _deck.Add(new CardData(suit, rank));
            
            // Fisher-Yates
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(0, i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
        }

        public CardData DealCard()
        {
            var c = _deck[_deck.Count - 1];
            _deck.RemoveAt(_deck.Count - 1);
            return c;
        }

        public CardData[] DealCards(int count)
        {
            var dealt = new CardData[count];
            for (int i = 0; i < count; i++) dealt[i] = DealCard();
            return dealt;
        }
    }
}
