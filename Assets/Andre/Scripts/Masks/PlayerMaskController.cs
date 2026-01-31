using Andre.Scripts.Masks.Base;
using Andre.Scripts.UI;
using UnityEngine;

namespace Andre.Scripts
{
    public class PlayerMaskController : MonoBehaviour
    {
        private MaskEffect _currentMask;
        private int _currentDuration;
        private PlayerMaskDisplay _worldDisplay;
        private PlayerMaskDisplayUI _uiDisplay;

        private void Awake()
        {
            _worldDisplay = GetComponentInChildren<PlayerMaskDisplay>();
        }

        private void Start()
        {
            FindMyUiDisplay();
            GameSystem.OnPlayerTurn += OnPlayerTurn;
        }

        private void OnDestroy()
        {
            GameSystem.OnPlayerTurn -= OnPlayerTurn;
        }

        private void FindMyUiDisplay()
        {
            var varTargetId = gameObject.name.Contains("1") ? 1 : 2;
            var varAllUis = FindObjectsByType<PlayerMaskDisplayUI>(FindObjectsSortMode.None);
            
            foreach (var varUi in varAllUis)
            {
                if (varUi.PlayerId == varTargetId)
                {
                    _uiDisplay = varUi;
                    break;
                }
            }
        }

        public void EquipMask(MaskEffect mask)
        {
            if (mask == null) return;

            _currentMask = mask;
            _currentDuration = mask.Duration;

            UpdateVisuals();
        }

        private void OnPlayerTurn()
        {
            if (_currentMask == null) return;

            _currentDuration--;

            if (_currentDuration <= 0)
            {
                RemoveMask();
            }
            else
            {
                UpdateVisuals();
            }
        }

        private void RemoveMask()
        {
            _currentMask = null;
            _currentDuration = 0;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var varSprite = _currentMask != null ? _currentMask.MaskSprite : null;

            if (_worldDisplay != null)
            {
                _worldDisplay.SetMask(varSprite);
            }

            if (_uiDisplay != null)
            {
                _uiDisplay.SetMask(varSprite, _currentDuration);
            }
        }
    }
}