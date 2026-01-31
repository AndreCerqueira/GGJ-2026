using UnityEngine;
using UnityEngine.UI;

namespace Andre.Scripts.UI
{
    public class PlayerMaskDisplayUI : MonoBehaviour
    {
        [SerializeField] private Image _maskImage;
        [SerializeField] private int _playerId;

        public int PlayerId => _playerId;

        public void SetMask(Sprite maskSprite)
        {
            if (_maskImage == null) return;

            if (maskSprite == null)
            {
                _maskImage.enabled = false;
                return;
            }

            _maskImage.sprite = maskSprite;
            _maskImage.enabled = true;
        }
    }
}