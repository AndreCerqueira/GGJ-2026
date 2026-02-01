using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Andre.Scripts.UI
{
    public class TextButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _targetContainer;
        [SerializeField] private TextMeshProUGUI _targetText;

        [Header("Scale Settings")]
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _animDuration = 0.2f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;

        [Header("Shake Settings")]
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeStrength = 5f;
        [SerializeField] private int _shakeVibrato = 15;

        [Header("Color Settings")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _hoverColor = new Color(1f, 0.8f, 0f); // Gold
        [SerializeField] private Color _clickColor = Color.red;

        private Vector3 _initialScale;
        private const float RESTORE_DELAY = 0.1f;

        private void Awake()
        {
            if (_targetContainer == null) 
                _targetContainer = GetComponent<RectTransform>();
            
            if (_targetText == null) 
                _targetText = GetComponentInChildren<TextMeshProUGUI>();

            _initialScale = _targetContainer.localScale;
        }

        private void Start()
        {
            if (_targetText != null)
                _targetText.color = _defaultColor;
        }

        private void OnDisable()
        {
            _targetContainer.DOKill();
            _targetText.DOKill();

            _targetContainer.localScale = _initialScale;
            if (_targetText != null)
                _targetText.color = _defaultColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetContainer.DOScale(_initialScale * _hoverScale, _animDuration)
                .SetEase(_scaleEase);
            
            _targetText.DOColor(_hoverColor, _animDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetContainer.DOScale(_initialScale, _animDuration);
            _targetText.DOColor(_defaultColor, _animDuration);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _targetContainer.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _shakeVibrato);

            _targetText.DOColor(_clickColor, 0.05f)
                .OnComplete(() => 
                {
                    _targetText.DOColor(_hoverColor, _animDuration).SetDelay(RESTORE_DELAY);
                });
        }
    }
}