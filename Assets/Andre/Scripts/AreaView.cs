using Andre.Scripts.Systems;
using UnityEngine;
using DG.Tweening;

namespace Andre.Scripts
{
    public class AreaView : MonoBehaviour
    {
        [SerializeField] private Transform _characterContainer;
        [SerializeField] private Transform _maskContainer;
        [SerializeField] private MeshRenderer _renderer;

        [Header("Textures")]
        [SerializeField] private Texture _defaultTexture;
        [SerializeField] private Texture _highlightTexture;
        [SerializeField] private Texture _selectedTexture;
        
        private Color _originalColor;
        private Vector2Int _coordinate;
        private Tween _blinkTween;

        public Transform CharacterContainer => _characterContainer;
        public Transform MaskContainer => _maskContainer;
        public Vector2Int Coordinate => _coordinate;
        public bool IsOccupied => _characterContainer.childCount > 0;
        public bool HasObstacle => _characterContainer.GetComponentInChildren<ObstacleView>() != null;
        public bool HasMask => _maskContainer.childCount > 0;

        private void Awake()
        {
            _originalColor = _renderer.material.color;
            UpdateTexture(false);
        }

        public void Setup(Vector2Int coord) => _coordinate = coord;

        public GameObject GetMask() => HasMask ? _maskContainer.GetChild(0).gameObject : null;

        public void SetHighlight(bool active, bool isCurrentPos = false)
        {
            _blinkTween?.Kill();
            _renderer.material.color = _originalColor;
            
            if (active)
            {
                _renderer.material.mainTexture = isCurrentPos ? _selectedTexture : _highlightTexture;
                /*
                _blinkTween = _renderer.material.DOColor(Color.cyan, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                    */
            }
            else
            {
                _renderer.material.mainTexture = _defaultTexture;
            }
        }

        private void UpdateTexture(bool isHighlight)
        {
            if (_renderer == null) return;
            var varTargetTexture = isHighlight ? _highlightTexture : _defaultTexture;
            if (varTargetTexture != null) _renderer.material.mainTexture = varTargetTexture;
        }

        private void OnMouseDown() => AreaMovementSystem.Instance.OnAreaClicked(this);

        private void OnDestroy() => _blinkTween?.Kill();
    }
}