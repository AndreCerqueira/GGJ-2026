using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts.Systems
{
    public class AreaMovementSystem : MonoBehaviour
    {
        public static AreaMovementSystem Instance { get; private set; }

        private GameSystem _gameSystem;

        private PlayerView _selectedPlayer;
        private List<AreaView> _highlightedAreas = new List<AreaView>();

        public int playerMoves = 3;

        private const float _moveDuration = 0.3f;

        public void Awake()
        {
            Instance = this;
            // Try to cache GameSystem reference; if it's not ready, register to get it when initialized
            _gameSystem = GameSystem.GetOrFindInstance();
            if (_gameSystem == null)
            {
                GameSystem.RegisterOnInitialized(() => { _gameSystem = GameSystem.Instance; });
            }
        }

        public void SelectPlayer(PlayerView player)
        {
            ClearHighlights();
            _selectedPlayer = player;
            
            var currentArea = player.GetComponentInParent<AreaView>();
            if (currentArea != null && playerMoves > 0)
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
            for (var i = 0; i < playerMoves; i++)
            {
                var playerTransform = _selectedPlayer.transform;
            
            playerTransform.SetParent(area.CharacterContainer);
            playerTransform.DOLocalMove(Vector3.zero, _moveDuration)
                           .SetEase(Ease.OutQuad)
                           .OnComplete(() => TryPickMask(area));

            ClearHighlights();
            _selectedPlayer = null;
            playerMoves -= 1;
            }

           // Debug.Log("[AreaMovementSystem] MovePlayerToArea completed - attempting to call EnemyTurn()");
            // Prefer cached reference
            var gs = _gameSystem ?? GameSystem.GetOrFindInstance();
            if (gs == null)
            {
               // Debug.LogError("[AreaMovementSystem] No GameSystem found in scene. EnemyTurn not called.");
                return;
            }

            //Debug.Log("[AreaMovementSystem] Calling GameSystem.Instance.EnemyTurn()");
            gs.EnemyTurn();
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