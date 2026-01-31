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
        private AreaView _currentSelectedArea;
        private List<AreaView> _highlightedAreas = new List<AreaView>();

        public int playerMoves = 3;

        private const float _moveDuration = 0.4f;
        private const float _scaleMultiplier = 1.05f;
        private const Ease _cartoonEase = Ease.OutBack;

        public void Awake()
        {
            Instance = this;
            _gameSystem = GameSystem.GetOrFindInstance();
            if (_gameSystem == null)
            {
                GameSystem.RegisterOnInitialized(() => { _gameSystem = GameSystem.Instance; });
            }
        }

        public void SelectPlayer(PlayerView player)
        {
            if (_gameSystem != null && _gameSystem.state != GameState.PLAYERTURN) return;

            if (_selectedPlayer != null) ResetPlayerScale(_selectedPlayer);

            ClearHighlights();
            _selectedPlayer = player;
            
            _selectedPlayer.transform.DOScale(Vector3.one * _scaleMultiplier, 0.2f).SetEase(Ease.OutBack);
            
            var varCurrentArea = player.GetComponentInParent<AreaView>();
            if (varCurrentArea != null)
            {
                _currentSelectedArea = varCurrentArea;
                _currentSelectedArea.SetHighlight(true, true);
                
                if (playerMoves > 0) ShowAdjacentAreas(varCurrentArea.Coordinate);
            }
        }

        private void ShowAdjacentAreas(Vector2Int center)
        {
            var varDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var varDir in varDirections)
            {
                var varTargetCoord = center + varDir;
                if (GridSystem.Instance.TryGetArea(varTargetCoord, out var varArea) && !varArea.IsOccupied)
                {
                    varArea.SetHighlight(true, false);
                    _highlightedAreas.Add(varArea);
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
            if (_selectedPlayer == null) return;

            ClearHighlights();
            _selectedPlayer.transform.SetParent(area.CharacterContainer);
            playerMoves--;

            _selectedPlayer.transform.DOLocalMove(Vector3.zero, _moveDuration)
                .SetEase(_cartoonEase)
                .OnComplete(() =>
                {
                    TryPickMask(area);
                    if (playerMoves <= 0) Deselect();
                    else SelectPlayer(_selectedPlayer);
                });
        }

        private void Deselect()
        {
            if (_selectedPlayer != null) ResetPlayerScale(_selectedPlayer);
            _selectedPlayer = null;
            ClearHighlights();
        }

        private void ResetPlayerScale(PlayerView player)
        {
            player.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InBack);
        }

        public void PassTurnManual()
        {
            var varGs = _gameSystem ?? GameSystem.GetOrFindInstance();
            if (varGs != null && varGs.state != GameState.PLAYERTURN) return;

            Deselect();
            playerMoves = 3; 
            if (varGs != null) varGs.EnemyTurn();
        }

        private void TryPickMask(AreaView area)
        {
            if (area.HasMask)
            {
                var varMask = area.GetMask();
                if (varMask != null) MaskSpawnerSystem.Instance.OnMaskPicked(varMask, area);
            }
        }

        private void ClearHighlights()
        {
            if (_currentSelectedArea != null) _currentSelectedArea.SetHighlight(false);
            _currentSelectedArea = null;

            foreach (var varArea in _highlightedAreas) varArea.SetHighlight(false);
            _highlightedAreas.Clear();
        }
    }
}