using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokerGame.Core
{
    [Serializable]
    public class CardVisualEntry
    {
        public Rank Rank;
        public Suit Suit;
        public Sprite CardSprite;
        public Material CardMaterial; // Useful for 3D
    }

    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Poker/CardDatabase")]
    public class CardDatabaseSO : ScriptableObject
    {
        public List<CardVisualEntry> Visuals = new List<CardVisualEntry>();
        public Sprite CardBackSprite;
        public Material CardBackMaterial;

        public Sprite GetSprite(Rank rank, Suit suit)
        {
            var entry = Visuals.Find(v => v.Rank == rank && v.Suit == suit);
            return entry?.CardSprite;
        }

        public Material GetMaterial(Rank rank, Suit suit)
        {
            var entry = Visuals.Find(v => v.Rank == rank && v.Suit == suit);
            return entry?.CardMaterial;
        }
    }
}
