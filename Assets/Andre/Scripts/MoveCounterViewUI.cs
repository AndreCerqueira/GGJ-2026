using Andre.Scripts.Systems;
using DG.Tweening; // Importante: namespace do DOTween
using TMPro;
using UnityEngine;

namespace Andre.Scripts
{
    public class MoveCounterViewUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private string _prefix = "Moves: ";

        [Header("Animation Settings")]
        [SerializeField] private float _shakeDuration = 0.4f;
        [SerializeField] private float _shakeStrength = 0.5f;
        [SerializeField] private int _shakeVibrato = 15;

        private int _lastKnownMoves = -1;
        private Vector3 _initialScale;

        private void Start()
        {
            // Guarda a escala original para garantir que volta ao normal após o shake
            if (_movesText != null)
            {
                _initialScale = _movesText.transform.localScale;
            }
        }

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

            if (_movesText != null)
            {
                // 1. Mata tweens anteriores para não acumular
                _movesText.transform.DOKill();
                
                // 2. Reseta a escala para a original antes de tremer
                _movesText.transform.localScale = _initialScale;

                // 3. Aplica o Scale Shake
                _movesText.transform.DOShakeScale(_shakeDuration, _shakeStrength, _shakeVibrato);
            }
        }
    }
}