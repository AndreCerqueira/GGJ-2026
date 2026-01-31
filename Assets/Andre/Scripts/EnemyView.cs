using System.Collections.Generic;
using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class EnemyView : MonoBehaviour
    {
        private const int MOVES_PER_TURN = 2;
        private const float MOVE_DURATION = 0.25f;
        private const float MOVE_DELAY = 0.1f;

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
            PerformTurnSequence();
        }

        private void PerformTurnSequence()
        {
            var gs = GameSystem.GetOrFindInstance();
            if (gs != null) gs.RegisterTurnAction();

            var sequence = DOTween.Sequence();

            for (var i = 0; i < MOVES_PER_TURN; i++)
            {
                sequence.AppendCallback(() => MoveTowardsNearestPlayer());
                sequence.AppendInterval(MOVE_DURATION + MOVE_DELAY);
            }

            sequence.OnComplete(() =>
            {
                if (gs != null) gs.CompleteTurnAction();
            });
        }

        private void MoveTowardsNearestPlayer()
        {
            var currentArea = GetComponentInParent<AreaView>();
            if (currentArea == null) return;

            var targetPlayer = GetNearestPlayer(currentArea.Coordinate);
            if (targetPlayer == null) return;

            var currentCoord = currentArea.Coordinate;
            var targetCoord = targetPlayer.GetComponentInParent<AreaView>().Coordinate;

            var direction = GetBestDirection(currentCoord, targetCoord);
            var nextCoord = currentCoord + direction;

            if (GridSystem.Instance.TryGetArea(nextCoord, out var nextArea))
            {
                MoveToArea(nextArea);
            }
        }

        private void MoveToArea(AreaView targetArea)
        {
            // Security check: if somehow we try to move to an obstacle, abort
            if (IsBlockedByObstacle(targetArea)) return;

            if (targetArea.IsOccupied)
            {
                var playersInArea = targetArea.CharacterContainer.GetComponentsInChildren<PlayerView>(true);
                foreach (var p in playersInArea)
                {
                    var hs = p.GetComponent<HealthSystem>();
                    if (hs != null) hs.Kill();
                }
            }

            transform.SetParent(targetArea.CharacterContainer);
            transform.DOLocalMove(Vector3.zero, MOVE_DURATION).SetEase(Ease.OutQuad);
        }

        private PlayerView GetNearestPlayer(Vector2Int currentCoord)
        {
            PlayerView nearest = null;
            var minDistance = float.MaxValue;

            foreach (var player in PlayerView.AllPlayers)
            {
                var playerArea = player.GetComponentInParent<AreaView>();
                if (playerArea == null) continue;

                var dist = Vector2Int.Distance(currentCoord, playerArea.Coordinate);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = player;
                }
            }

            return nearest;
        }

        private Vector2Int GetBestDirection(Vector2Int current, Vector2Int target)
        {
            var diff = target - current;
            var tryHorizontal = Mathf.Abs(diff.x) > Mathf.Abs(diff.y);

            if (tryHorizontal)
            {
                var dirX = new Vector2Int(System.Math.Sign(diff.x), 0);
                if (IsValidMove(current + dirX)) return dirX;

                var dirY = new Vector2Int(0, System.Math.Sign(diff.y));
                if (IsValidMove(current + dirY)) return dirY;
            }
            else
            {
                var dirY = new Vector2Int(0, System.Math.Sign(diff.y));
                if (IsValidMove(current + dirY)) return dirY;

                var dirX = new Vector2Int(System.Math.Sign(diff.x), 0);
                if (IsValidMove(current + dirX)) return dirX;
            }

            return Vector2Int.zero;
        }

        private bool IsValidMove(Vector2Int coord)
        {
            if (!GridSystem.Instance.TryGetArea(coord, out var area)) 
                return false;
            
            return !IsBlockedByObstacle(area);
        }

        private bool IsBlockedByObstacle(AreaView area)
        {
            if (!area.IsOccupied) return false;

            var obstacle = area.CharacterContainer.GetComponentInChildren<ObstacleView>();
            return obstacle != null;
        }
    }
}