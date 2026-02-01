using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Andre.Scripts
{
    public class InitialScreenViewUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _startButton;

        [SerializeField] private AreaViewCreator _areaViewCreator;
        [SerializeField] private ToastPresetSO _onboardingPreset;
        
        [Header("Feedbacks")]
        [SerializeField] private MMF_Player _gameStartFeedback;
        
        private void Awake()
        {
            _startButton.onClick.AddListener(OnScreenButtonClicked);
        }
        
        private void OnScreenButtonClicked()
        {
            _gameStartFeedback?.PlayFeedbacks();
            _areaViewCreator.Initialize(false);
            
            DOVirtual.DelayedCall(1.0f, () =>
            {
                ToastSystem.Instance.Show("You need to find the exit!", _onboardingPreset);
            });
        }
    }
}
