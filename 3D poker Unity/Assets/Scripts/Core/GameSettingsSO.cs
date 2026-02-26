using UnityEngine;

namespace PokerGame.Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Poker/GameSettings")]
    public class GameSettingsSO : ScriptableObject
    {
        [Header("Match Rules")]
        public int PlayerCount = 4;
        public int StartingChips = 1000;
        public int SmallBlindAmount = 10;
        public int BigBlindAmount = 20;

        [Header("Time Limits")]
        public float TurnTimeLimit = 15f;
    }
}
