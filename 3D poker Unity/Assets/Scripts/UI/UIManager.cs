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
        [SerializeField] private CardDatabaseSO _cardDatabase;

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
            if (_seats != null && _gm != null)
            {
                for (int i = 0; i < _seats.Length && i < _gm.PlayerCount; i++)
                    if (_seats[i] != null) _seats[i].Init(_gm.Players[i]);
            }
            OnRestart();
        }

        public void RestartGameBtn() => _gm?.RestartMatch();

        // ── Events ──
        private void OnStateChanged(GameState s) { if (_stateLabel != null) _stateLabel.text = s.ToString(); }
        private void OnPot(int p) { if (_potLabel != null) _potLabel.text = $"Pot: ${p}"; }
        private void OnChips(int id, int c) { if(_seats != null && id < _seats.Length && _seats[id] != null) _seats[id].SetChips(c); }
        private void OnTick(int id, float t) 
        { 
            if(_seats != null && id < _seats.Length && _seats[id] != null) 
                _seats[id].SetTimer(t); 
            else
                Debug.LogWarning($"[UIManager] Cannot set timer for ID {id}. Seats count: {_seats?.Length ?? 0}");
        }
        private void OnTurn(int id) 
        { 
            Debug.Log($"[UIManager] Turn changed to ID {id}");
            if (_seats == null) return;
            foreach(var s in _seats) if(s != null) s.SetActive(false); 
            if(id < _seats.Length && _seats[id] != null) _seats[id].SetActive(true); 
        }
        private void OnAction(int id, PlayerAction a, int amt) { if(_seats != null && id < _seats.Length && _seats[id] != null) _seats[id].SetAction(a, amt); }
        private void OnHoleCards(int id, CardData[] c) { if(_seats != null && id < _seats.Length && _seats[id] != null && _gm != null) _seats[id].SetHoleCards(c, !(_gm.Players[id].IsAI)); }
        
        private void OnCommunityCards(CardData[] cards)
        {
            if (_communityCards == null || _gm == null) return;
            var all = _gm.CommunityCards;
            for(int i = 0; i < _communityCards.Length; i++)
            {
                if (_communityCards[i] == null) continue;
                if(i < all.Count) 
                {
                    _communityCards[i].color = Color.white;
                    if (_cardDatabase != null)
                    {
                        var sprite = _cardDatabase.GetSprite(all[i].Rank, all[i].Suit);
                        if (sprite != null) _communityCards[i].sprite = sprite;
                    }
                }
                else 
                {
                    _communityCards[i].color = Color.gray;
                    if (_cardDatabase != null)
                    {
                        if (_cardDatabase.CardBackSprite != null) _communityCards[i].sprite = _cardDatabase.CardBackSprite;
                    }
                }
            }
        }

        private void OnEnded(int[] w, int p)
        {
            if (_gm == null) return;
            var names = string.Join(" & ", w.Select(id => _gm.Players[id].Name));
            if (_winnerLabel != null)
            {
                _winnerLabel.text = $"{names} won ${p}!";
                _winnerLabel.gameObject.SetActive(true);
            }
            if (_seats != null) foreach(var s in _seats) if (s != null) s.RevealAI();
        }

        private void OnRestart()
        {
            if (_potLabel != null) _potLabel.text = "Pot: $0";
            if (_stateLabel != null) _stateLabel.text = "PreFlop";
            if (_winnerLabel != null) 
            {
                _winnerLabel.text = "";
                _winnerLabel.gameObject.SetActive(false);
            }
            EventBus.TotalPotUpdated(0);
            if (_seats != null) 
            {
                for (int i = 0; i < _seats.Length; i++)
                {
                    if (_seats[i] == null) continue;
                    _seats[i].ResetUI();
                    if (_gm != null && i < _gm.Players.Count)
                        _seats[i].SetChips(_gm.Players[i].Chips);
                }
            }
            if (_communityCards != null) foreach(var img in _communityCards) if (img != null) img.color = Color.gray;
        }
    }
}
