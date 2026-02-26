using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokerGame.Core;

namespace PokerGame.Visuals
{
    /// <summary>
    /// Connects the headless Poker logic to 3D GameObjects in the scene.
    /// Listens to EventBus and spawns/moves 3D cards and chips.
    /// </summary>
    public class Table3DVisuals : MonoBehaviour
    {
        [Header("3D Prefabs (Use Cubes/Quads if no models)")]
        public GameObject CardPrefab3D;
        public GameObject ChipStackPrefab3D;

        [Header("Table Positions")]
        public Transform[] CommunityCardSpots; // 5 spots in center of table
        public Transform PotCenterSpot;        // Where chips go when betting
        
        [Header("Player Positions")]
        public Transform[] PlayerSeats3D;      // Where the 3D players sit
        public Transform[] CardSpawnPoints;    // Array of 2 points per player

        private List<GameObject> _spawnedCards = new List<GameObject>();
        private List<GameObject> _spawnedChips = new List<GameObject>();
        private Dictionary<int, List<GameObject>> _playerCards = new Dictionary<int, List<GameObject>>();
        private List<GameObject> _communityCards = new List<GameObject>();

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += OnStateChanged;
            EventBus.OnHoleCardsDealt += DealHoleCards3D;
            EventBus.OnCommunityCardsRevealed += SpawnCommunityCards3D;
            EventBus.OnPlayerActionTaken += SpawnBetChips3D;
            EventBus.OnRoundEnded += MovePotToWinner3D;
            EventBus.OnGameRestarted += ClearTable3D;
            EventBus.OnHandEvaluated += OnHandEvaluated; // Listen for reveal
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= OnStateChanged;
            EventBus.OnHoleCardsDealt -= DealHoleCards3D;
            EventBus.OnCommunityCardsRevealed -= SpawnCommunityCards3D;
            EventBus.OnPlayerActionTaken -= SpawnBetChips3D;
            EventBus.OnRoundEnded -= MovePotToWinner3D;
            EventBus.OnGameRestarted -= ClearTable3D;
            EventBus.OnHandEvaluated -= OnHandEvaluated;
        }

        // ── 3D Visual Reactions ──

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.PreFlop) ClearTable3D();
        }

        private void DealHoleCards3D(int playerId, CardData[] cards)
        {
            if (CardPrefab3D == null || CardSpawnPoints == null) return;

            if (!_playerCards.ContainsKey(playerId)) _playerCards[playerId] = new List<GameObject>();

            for (int i = 0; i < 2; i++)
            {
                int pointIndex = (playerId * 2) + i;
                
                if (pointIndex < CardSpawnPoints.Length && CardSpawnPoints[pointIndex] != null)
                {
                    Transform spawnPoint = CardSpawnPoints[pointIndex];
                    GameObject cardObj = Instantiate(CardPrefab3D, spawnPoint.position, spawnPoint.rotation);
                    
                    _spawnedCards.Add(cardObj);
                    _playerCards[playerId].Add(cardObj);

                    // Smooth Flip for human player (ID 0)
                    if (playerId == 0) 
                    {
                        StartCoroutine(FlipCard(cardObj.transform, 0.5f));
                    }
                }
            }
        }

        private void SpawnCommunityCards3D(CardData[] cards)
        {
            if (CardPrefab3D == null || CommunityCardSpots == null) return;

            
            int alreadySpawned = _communityCards.Count;
            
            for (int i = 0; i < cards.Length; i++)
            {
                int spotIndex = alreadySpawned + i;
                if (spotIndex < CommunityCardSpots.Length)
                {
                    Transform spot = CommunityCardSpots[spotIndex];
                    var cardObj = Instantiate(CardPrefab3D, spot.position, spot.rotation);
                    _spawnedCards.Add(cardObj);
                    _communityCards.Add(cardObj);
                    
                    // Smooth Flip Reveal
                    StartCoroutine(FlipCard(cardObj.transform, 0.5f));
                }
            }
        }

        private void OnHandEvaluated(int playerId, PokerHand hand)
        {
            // At Showdown, reveal AI players' cards
            if (playerId != 0 && _playerCards.ContainsKey(playerId))
            {
                foreach (var cardObj in _playerCards[playerId])
                {
                    // Check if already flipped (basic check: rotation X near 180)
                    if (Mathf.Abs(Quaternion.Angle(cardObj.transform.localRotation, Quaternion.Euler(180, 0, 0))) > 10)
                    {
                        StartCoroutine(FlipCard(cardObj.transform, 0.5f));
                    }
                }
            }
        }

        private void SpawnBetChips3D(int playerId, PlayerAction action, int amount)
        {
            if (amount <= 0 || ChipStackPrefab3D == null || PotCenterSpot == null) return;
            
            // Spawn a chip stack and throw it to the center
            Transform seat = PlayerSeats3D[playerId];
            var chipsObj = Instantiate(ChipStackPrefab3D, seat.position, Quaternion.identity);
            
            // Simple animation: move to center
            StartCoroutine(MoveObj(chipsObj.transform, PotCenterSpot.position + Vector3.up * (_spawnedChips.Count * 0.1f), 0.5f));
            _spawnedChips.Add(chipsObj);
        }

        private void MovePotToWinner3D(int[] winnerIds, int potAmount)
        {
            if (winnerIds.Length == 0) return;
            
            Transform winnerSeat = PlayerSeats3D[winnerIds[0]]; // Simplify to first winner
            
            foreach (var chip in _spawnedChips)
            {
                if (chip != null)
                    StartCoroutine(MoveObj(chip.transform, winnerSeat.position, 1f));
            }
        }

        private void ClearTable3D()
        {
            StopAllCoroutines();
            foreach (var c in _spawnedCards) if (c) Destroy(c);
            foreach (var c in _spawnedChips) if (c) Destroy(c);
            _spawnedCards.Clear();
            _spawnedChips.Clear();
            _playerCards.Clear();
            _communityCards.Clear();
        }

        private IEnumerator FlipCard(Transform card, float duration)
        {
            Quaternion startRot = card.rotation;
            Quaternion endRot = startRot * Quaternion.Euler(180, 0, 0);
            float elapsed = 0;
            while (elapsed < duration && card != null)
            {
                card.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (card != null) card.rotation = endRot;
        }

        private IEnumerator MoveObj(Transform obj, Vector3 target, float duration)
        {
            Vector3 startPath = obj.position;
            float elapsed = 0;
            while(elapsed < duration && obj != null)
            {
                obj.position = Vector3.Lerp(startPath, target, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (obj != null) obj.position = target;
        }
    }
}
