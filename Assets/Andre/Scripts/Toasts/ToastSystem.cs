using System.Collections.Generic;
using Andre.Scripts.Toasts;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts.UI
{
    public class ToastSystem : MonoBehaviour
    {
        public static ToastSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private ToastViewUI _viewUI;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;

        [Header("Default Settings")]
        [SerializeField] private ToastPresetSO _defaultPreset;

        [Header("Animation")]
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private float _moveDistance = 50f;

        // Estrutura interna para a fila
        private struct ToastRequest
        {
            public string Message;
            public ToastPresetSO Preset;
        }

        private readonly Queue<ToastRequest> _queue = new Queue<ToastRequest>();
        private bool _isShowing;
        private Vector2 _originalAnchoredPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Inicialização visual
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            
            if (_rectTransform)
                _originalAnchoredPosition = _rectTransform.anchoredPosition;
        }

        /// <summary>
        /// Mostra um toast usando uma mensagem específica e um preset visual.
        /// </summary>
        public void Show(string message, ToastPresetSO preset = null)
        {
            var p = preset != null ? preset : _defaultPreset;
            
            _queue.Enqueue(new ToastRequest 
            { 
                Message = message, 
                Preset = p 
            });

            if (!_isShowing)
                ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_queue.Count == 0)
            {
                _isShowing = false;
                return;
            }

            _isShowing = true;
            var request = _queue.Dequeue();

            // Configura a View
            _viewUI.Setup(request.Message, request.Preset);

            // Reseta animações anteriores
            _rectTransform.DOKill();
            _canvasGroup.DOKill();

            // Posição inicial (levemente abaixo ou acima para entrar deslizando)
            _rectTransform.anchoredPosition = _originalAnchoredPosition - new Vector2(0, _moveDistance);
            _canvasGroup.alpha = 0f;

            var seq = DOTween.Sequence();

            // Animação de Entrada
            seq.Append(_canvasGroup.DOFade(1f, _animationDuration));
            seq.Join(_rectTransform.DOAnchorPos(_originalAnchoredPosition, _animationDuration).SetEase(Ease.OutBack));

            // Tempo de espera
            seq.AppendInterval(request.Preset.DisplayDuration);

            // Animação de Saída
            seq.Append(_canvasGroup.DOFade(0f, _animationDuration));
            seq.Join(_rectTransform.DOAnchorPos(_originalAnchoredPosition + new Vector2(0, _moveDistance), _animationDuration));

            // Chama o próximo da fila
            seq.OnComplete(ProcessQueue);
        }
    }
}