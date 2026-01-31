using Andre.Scripts.Systems;
using TMPro;
using UnityEngine;

namespace Andre.Scripts
{
    public class MoveCounterViewUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private string _prefix = "Moves: ";

        private int _lastKnownMoves = -1;

        private void Update()
        {
            if (AreaMovementSystem.Instance == null) return;

            var currentMoves = AreaMovementSystem.Instance.playerMoves;

            if (currentMoves != _lastKnownMoves)
            {
                UpdateLabel(currentMoves);
            }
        }

        private void UpdateLabel(int moves)
        {
            _lastKnownMoves = moves;
            _movesText.text = $"{_prefix}{moves}";
        }
    }
}