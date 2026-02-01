using UnityEngine;
using UnityEngine.UI;
using Andre.Scripts.Systems;

namespace Andre.Scripts
{
    public class EndTurnButtonViewUI : MonoBehaviour
    {
        [SerializeField] private Button _endTurnButton;

        private void Awake()
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
        }

        private void Start()
        {
            var gs = GameSystem.GetOrFindInstance();
            if (gs != null)
            {
                UpdateState(gs.state == GameState.PLAYERTURN);
            }
        }

        private void OnEnable()
        {
            GameSystem.Instance.OnPlayerTurn += OnPlayerTurnStarted;
            GameSystem.Instance.OnEnemyTurn += OnEnemyTurnStarted;
        }

        private void OnDisable()
        {
            GameSystem.Instance.OnPlayerTurn -= OnPlayerTurnStarted;
            GameSystem.Instance.OnEnemyTurn -= OnEnemyTurnStarted;
        }

        private void OnPlayerTurnStarted()
        {
            UpdateState(true);
        }

        private void OnEnemyTurnStarted()
        {
            UpdateState(false);
        }

        private void OnEndTurnClicked()
        {
            UpdateState(false);
            
            if (AreaMovementSystem.Instance != null)
            {
                AreaMovementSystem.Instance.PassTurnManual();
            }
        }

        private void UpdateState(bool isInteractable)
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = isInteractable;
            }
        }
    }
}