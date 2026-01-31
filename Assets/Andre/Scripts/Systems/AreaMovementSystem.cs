using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts.Systems
{
    public class AreaMovementSystem : MonoBehaviour
    {
        public static AreaMovementSystem Instance { get; private set; }

        private PlayerView _selectedPlayer;
        private List<AreaView> _highlightedAreas = new List<AreaView>();

        private const float _moveDuration = 0.3f;

        private void Awake()
        {
            Instance = this;
        }

        public void SelectPlayer(PlayerView player)
        {
            ClearHighlights();
            _selectedPlayer = player;
            
            var currentArea = player.GetComponentInParent<AreaView>();
            if (currentArea != null)
            {
                ShowAdjacentAreas(currentArea.Coordinate);
            }
        }

        private void ShowAdjacentAreas(Vector2Int center)
        {
            var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                var targetCoord = center + dir;
                if (GridSystem.Instance.TryGetArea(targetCoord, out var area) && !area.IsOccupied)
                {
                    area.SetHighlight(true);
                    _highlightedAreas.Add(area);
                }
            }
        }

        public void OnAreaClicked(AreaView area)
        {
            if (_selectedPlayer == null || !_highlightedAreas.Contains(area)) return;

            MovePlayerToArea(area);
        }

        public void MovePlayerToArea(AreaView area)
        {
            var playerTransform = _selectedPlayer.transform;
            
            playerTransform.SetParent(area.CharacterContainer);
            playerTransform.DOLocalMove(Vector3.zero, _moveDuration)
                           .SetEase(Ease.OutQuad)
                           .OnComplete(() => TryPickMask(area));

            ClearHighlights();
            _selectedPlayer = null;
        }

        private void TryPickMask(AreaView area)
        {
            if (area.HasMask)
            {
                var mask = area.GetMask();
                if (mask != null)
                {
                    MaskSpawnerSystem.Instance.OnMaskPicked(mask, area);
                }
            }
        }

        private void ClearHighlights()
        {
            foreach (var area in _highlightedAreas)
            {
                area.SetHighlight(false);
            }
            _highlightedAreas.Clear();
        }
    }
}