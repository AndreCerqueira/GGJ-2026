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
        
        [Header("Feedbacks")]
        [SerializeField] private MMF_Player _gameStartFeedback;
        
        private void Awake()
        {
            _startButton.onClick.AddListener(OnScreenButtonClicked);
        }
        
        private void OnScreenButtonClicked()
        {
            _gameStartFeedback?.PlayFeedbacks();
            _areaViewCreator.Initialize();
        }
    }
}
