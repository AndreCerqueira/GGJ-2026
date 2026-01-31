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
            // Tenta cachear a referência; se não estiver pronta, registra para pegar na inicialização
            _gameSystem = GameSystem.GetOrFindInstance();
            if (_gameSystem == null)
            {
                GameSystem.RegisterOnInitialized(() => { _gameSystem = GameSystem.Instance; });
            }
        }

        public void SelectPlayer(PlayerView player)
        {
            // Impede selecionar se não for o turno do jogador
            if (_gameSystem != null && _gameSystem.state != GameState.PLAYERTURN) return;

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
            
                    // ALTERAÇÃO AQUI: 
                    // Se acabaram os movimentos, apenas deselecionamos. 
                    // NÃO chamamos mais o EnemyTurn automaticamente.
                    if (playerMoves <= 0)
                    {
                        _selectedPlayer = null;
                        ClearHighlights();
                    }
                    else
                    {
                        // Ainda tem movimentos: Atualiza os highlights para a nova posição
                        ShowAdjacentAreas(area.Coordinate);
                    }
                });
        }

        /// <summary>
        /// Chamado pelo botão de UI para encerrar o turno manualmente.
        /// </summary>
        public void PassTurnManual()
        {
            var gs = _gameSystem ?? GameSystem.GetOrFindInstance();

            // Segurança: Só passa o turno se for realmente a vez do jogador
            if (gs != null && gs.state != GameState.PLAYERTURN) return;

            _selectedPlayer = null;
            ClearHighlights();
            
            // Reseta os movimentos para o próximo turno
            playerMoves = 3; 

            Debug.Log("[AreaMovementSystem] Turno passado manualmente via botão.");
            if (gs != null) gs.EnemyTurn();
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