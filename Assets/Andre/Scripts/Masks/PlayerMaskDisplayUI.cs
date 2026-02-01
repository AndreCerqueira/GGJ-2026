using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Andre.Scripts.UI
{
    public class PlayerMaskDisplayUI : MonoBehaviour
    {
        [SerializeField] private Image _maskImage;
        [SerializeField] private GameObject _counterContainer;
        [SerializeField] private TextMeshProUGUI _counterText;
        [SerializeField] private int _playerId;
        
        [SerializeField] private MMF_Player _hideFeedback;

        public int PlayerId => _playerId;

        public void SetMask(Sprite maskSprite, int duration)
        {
            if (_maskImage == null) return;

            if (maskSprite == null || duration <= 0)
            {
                _maskImage.enabled = false;
                _counterContainer.SetActive(false);
                return;
            }

            _maskImage.sprite = maskSprite;
            _maskImage.enabled = true;

            if (_counterText != null)
            {
                _counterText.text = duration.ToString();
                _counterContainer.SetActive(true);
            }
        }
        
        public void Hide()
        {
            _hideFeedback?.PlayFeedbacks();
        }
    }
}