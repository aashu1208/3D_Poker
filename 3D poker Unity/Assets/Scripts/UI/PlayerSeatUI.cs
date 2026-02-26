using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerGame.Core;

namespace PokerGame.UI
{
    public class PlayerSeatUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameTxt;
        [SerializeField] private TextMeshProUGUI _chipsTxt;
        [SerializeField] private TextMeshProUGUI _actionTxt;
        [SerializeField] private TextMeshProUGUI _timerTxt;
        [SerializeField] private Image _timerFill;
        [SerializeField] private GameObject _activeOutline;
        [SerializeField] private Image[] _holeCards;
        
        private PlayerData _p;

        public void Init(PlayerData p)
        {
            _p = p;
            if (_nameTxt != null) _nameTxt.text = p.Name;
            SetChips(p.Chips);
            ResetUI();
        }

        public void SetChips(int c) { if (_chipsTxt != null) _chipsTxt.text = $"${c}"; }
        
        public void SetActive(bool a) 
        { 
            if (_activeOutline != null) _activeOutline.SetActive(a); 
            if (_timerFill != null) _timerFill.fillAmount = a ? 1 : 0; 
            if (_timerTxt != null) _timerTxt.text = ""; // Clear text when turn starts/ends
        }
        
        public void SetTimer(float t) 
        { 
            if (_timerFill != null) _timerFill.fillAmount = t / 15f; 
            if (_timerTxt != null) _timerTxt.text = $"{Mathf.CeilToInt(t)}s";
            if (_timerFill == null && _timerTxt == null) Debug.LogWarning($"[PlayerSeatUI] Timer UI elements not assigned for {_p?.Name}");
        }
        
        public void SetAction(PlayerAction a, int amt)
        {
            if (_actionTxt != null) _actionTxt.text = a == PlayerAction.Raise ? $"RAISE {amt}" : a.ToString().ToUpper();
        }

        public void SetHoleCards(CardData[] c, bool reveal)
        {
            if (_holeCards == null) return;
            for(int i=0; i<_holeCards.Length && i<c.Length; i++)
            {
                if (_holeCards[i] != null)
                    _holeCards[i].color = reveal ? Color.white : Color.black;
            }
        }

        public void RevealAI()
        {
            if (_p != null && _p.HoleCards != null)
                SetHoleCards(_p.HoleCards, true);
        }

        public void ResetUI()
        {
            if (_actionTxt != null) _actionTxt.text = "";
            if (_timerTxt != null) _timerTxt.text = "";
            SetActive(false);
            if (_holeCards != null)
            {
                foreach(var img in _holeCards) 
                    if (img != null) img.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark neutral
            }
        }
    }
}
