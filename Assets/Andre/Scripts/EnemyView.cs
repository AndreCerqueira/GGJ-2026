using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class EnemyView : MonoBehaviour
    {
        // Direction in grid coordinates (right/left)
        private Vector2Int _direction = Vector2Int.right;
        private int _consecutiveMoves = 0;
        private const int MovesBeforeFlip = 4;

        private void OnEnable()
        {
            GameSystem.OnEnemyTurn += OnEnemyTurn;
        }

        private void OnDisable()
        {
            GameSystem.OnEnemyTurn -= OnEnemyTurn;
        }

        private void OnEnemyTurn()
        {
            // Move one tile in current direction each enemy turn.
            TryMoveOneTile();

            _consecutiveMoves++;
            if (_consecutiveMoves >= MovesBeforeFlip)
            {
                _consecutiveMoves = 1;
                _direction = new Vector2Int(-_direction.x, -_direction.y); // flip horizontal/vertical
            }
        }

        private void TryMoveOneTile()
        {
            var currentArea = GetComponentInParent<AreaView>();
            if (currentArea == null)
            {
                Debug.LogWarning($"[EnemyView] {gameObject.name} not parented under an AreaView; can't move.");
                return;
            }

            var targetCoord = currentArea.Coordinate + _direction;
            if (!GridSystem.Instance.TryGetArea(targetCoord, out var targetArea))
            {
                // If target out of bounds, flip direction and try the other way
                _direction = new Vector2Int(-_direction.x, -_direction.y);
                targetCoord = currentArea.Coordinate + _direction;
                if (!GridSystem.Instance.TryGetArea(targetCoord, out targetArea))
                {
                    Debug.LogWarning($"[EnemyView] {gameObject.name} cannot move in either direction from {currentArea.name}.");
                    return;
                }
            }

            // If there are players in the target area, kill them first (handles cases where physics collisions don't fire)
            if (targetArea.IsOccupied)
            {
                var playersInArea = targetArea.CharacterContainer.GetComponentsInChildren<Andre.Scripts.PlayerView>(true);
                foreach (var p in playersInArea)
                {
                    var hs = p.GetComponent<HealthSystem>();
                    if (hs != null)
                    {
                        Debug.Log($"[EnemyView] {gameObject.name} killing player {p.gameObject.name} in {targetArea.name}");
                        hs.Kill();
                    }
                }
            }

            // Reparent and animate to center
            transform.SetParent(targetArea.CharacterContainer);

            var gs = GameSystem.GetOrFindInstance();
            if (gs != null)
            {
                gs.RegisterTurnAction();
            }

            transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    Debug.Log($"[EnemyView] {gameObject.name} moved to {targetArea.name} at {targetCoord}");
                    if (gs != null) gs.CompleteTurnAction();
                });
        }
    }
}