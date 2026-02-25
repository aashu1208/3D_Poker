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
        [SerializeField] private Image _timerFill;
        [SerializeField] private GameObject _activeOutline;
        [SerializeField] private Image[] _holeCards;
        
        private PlayerData _p;

        public void Init(PlayerData p)
        {
            _p = p;
            _nameTxt.text = p.Name;
            SetChips(p.Chips);
            ResetUI();
        }

        public void SetChips(int c) => _chipsTxt.text = $"${c}";
        public void SetActive(bool a) { _activeOutline.SetActive(a); _timerFill.fillAmount = a ? 1 : 0; }
        public void SetTimer(float t) => _timerFill.fillAmount = t / 15f;
        
        public void SetAction(PlayerAction a, int amt)
        {
            _actionTxt.text = a == PlayerAction.Raise ? $"RAISE {amt}" : a.ToString().ToUpper();
        }

        public void SetHoleCards(CardData[] c, bool reveal)
        {
            for(int i=0; i<_holeCards.Length && i<c.Length; i++)
                _holeCards[i].color = reveal ? Color.white : Color.black; // Visual placeholder for hidden vs revealed
        }

        public void RevealAI()
        {
            if (_p != null && _p.HoleCards != null)
                SetHoleCards(_p.HoleCards, true);
        }

        public void ResetUI()
        {
            _actionTxt.text = "";
            SetActive(false);
            foreach(var img in _holeCards) img.color = Color.gray;
        }
    }
}
