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
            // 1. Verifica se temos jogador selecionado
            if (_selectedPlayer == null) return;

            var gs = _gameSystem ?? GameSystem.GetOrFindInstance();

            // 2. Limpa os highlights antigos
            ClearHighlights();

            // 3. Move o jogador para o novo contentor
            _selectedPlayer.transform.SetParent(area.CharacterContainer);

            // 4. Consome 1 movimento
            playerMoves--;

            // 5. Animação do movimento
            _selectedPlayer.transform.DOLocalMove(Vector3.zero, _moveDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    TryPickMask(area);
            
                    // Verifica se o turno acabou
                    if (playerMoves <= 0)
                    {
                        // Acabaram os movimentos: Passa o turno
                        _selectedPlayer = null;
                        playerMoves = 3; // Reset para o próximo turno
                
                        if (gs != null) gs.EnemyTurn();
                    }
                    else
                    {
                        // Ainda tem movimentos: Atualiza os highlights para a nova posição
                        ShowAdjacentAreas(area.Coordinate);
                    }
                });
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