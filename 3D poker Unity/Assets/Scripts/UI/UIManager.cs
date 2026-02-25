using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerGame.Core;
using PokerGame.Managers;

namespace PokerGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameManager _gm;
        [SerializeField] private PlayerSeatUI[] _seats;
        [SerializeField] private TextMeshProUGUI _potLabel;
        [SerializeField] private TextMeshProUGUI _stateLabel;
        [SerializeField] private TextMeshProUGUI _winnerLabel;
        [SerializeField] private Image[] _communityCards;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += OnStateChanged;
            EventBus.OnHoleCardsDealt += OnHoleCards;
            EventBus.OnCommunityCardsRevealed += OnCommunityCards;
            EventBus.OnTotalPotUpdated += OnPot;
            EventBus.OnChipsChanged += OnChips;
            EventBus.OnTurnChanged += OnTurn;
            EventBus.OnTurnTimerTick += OnTick;
            EventBus.OnPlayerActionTaken += OnAction;
            EventBus.OnRoundEnded += OnEnded;
            EventBus.OnGameRestarted += OnRestart;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= OnStateChanged;
            EventBus.OnHoleCardsDealt -= OnHoleCards;
            EventBus.OnCommunityCardsRevealed -= OnCommunityCards;
            EventBus.OnTotalPotUpdated -= OnPot;
            EventBus.OnChipsChanged -= OnChips;
            EventBus.OnTurnChanged -= OnTurn;
            EventBus.OnTurnTimerTick -= OnTick;
            EventBus.OnPlayerActionTaken -= OnAction;
            EventBus.OnRoundEnded -= OnEnded;
            EventBus.OnGameRestarted -= OnRestart;
        }

        private void Start()
        {
            for (int i = 0; i < _seats.Length && i < _gm.PlayerCount; i++)
                _seats[i].Init(_gm.Players[i]);
            OnRestart();
        }

        public void RestartGameBtn() => _gm.RestartMatch();

        // ── Events ──
        private void OnStateChanged(GameState s) => _stateLabel.text = s.ToString();
        private void OnPot(int p) => _potLabel.text = $"Pot: ${p}";
        private void OnChips(int id, int c) { if(id < _seats.Length) _seats[id].SetChips(c); }
        private void OnTurn(int id) { foreach(var s in _seats) s.SetActive(false); if(id < _seats.Length) _seats[id].SetActive(true); }
        private void OnTick(int id, float t) { if(id < _seats.Length) _seats[id].SetTimer(t); }
        private void OnAction(int id, PlayerAction a, int amt) { if(id < _seats.Length) _seats[id].SetAction(a, amt); }
        private void OnHoleCards(int id, CardData[] c) { if(id < _seats.Length) _seats[id].SetHoleCards(c, !(_gm.Players[id].IsAI)); }
        
        private void OnCommunityCards(CardData[] cards)
        {
            var all = _gm.CommunityCards;
            for(int i = 0; i < _communityCards.Length; i++)
            {
                if(i < all.Count) _communityCards[i].color = Color.white; // Set sprite based on all[i]
                else _communityCards[i].color = Color.gray;
            }
        }

        private void OnEnded(int[] w, int p)
        {
            var names = string.Join(" & ", w.Select(id => _gm.Players[id].Name));
            _winnerLabel.text = $"{names} won ${p}!";
            _winnerLabel.gameObject.SetActive(true);
            foreach(var s in _seats) s.RevealAI();
        }

        private void OnRestart()
        {
            _potLabel.text = "Pot: $0";
            _winnerLabel.gameObject.SetActive(false);
            EventBus.TotalPotUpdated(0);
            foreach(var s in _seats) s.ResetUI();
            foreach(var img in _communityCards) img.color = Color.gray;
        }
    }
}
