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

        public void Setup(Vector2Int coord)
        {
            _coordinate = coord;
        }

        public GameObject GetMask()
        {
            if (HasMask)
            {
                return _maskContainer.GetChild(0).gameObject;
            }
            return null;
        }

        public void SetHighlight(bool active)
        {
            _blinkTween?.Kill();
            _renderer.material.color = _originalColor;
            UpdateTexture(active);

            if (active)
            {
                _blinkTween = _renderer.material.DOColor(Color.cyan, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void UpdateTexture(bool isHighlight)
        {
            if (_renderer == null) return;

            var varTargetTexture = isHighlight ? _highlightTexture : _defaultTexture;
            if (varTargetTexture != null)
            {
                _renderer.material.mainTexture = varTargetTexture;
            }
        }

        private void OnMouseDown()
        {
            AreaMovementSystem.Instance.OnAreaClicked(this);
        }

        private void OnDestroy()
        {
            _blinkTween?.Kill();
        }
    }
}