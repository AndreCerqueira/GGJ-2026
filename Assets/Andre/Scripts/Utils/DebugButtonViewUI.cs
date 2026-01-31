using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Utils
{
    [RequireComponent(typeof(Button))]
    public class DebugButtonViewUI : MonoBehaviour
    {
        [SerializeField] private MMF_Player _showFeedback;
        [SerializeField] private MMF_Player _hideFeedback;
        
        private Button _button;
        private bool _isVisible = false;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ToggleVisibility);
        }
        
        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
            {
                _showFeedback?.PlayFeedbacks();
            }
            else
            {
                _hideFeedback?.PlayFeedbacks();
            }
        }
    }
}
