using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Andre.Scripts.Masks;
using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class EnemyView : MonoBehaviour
    {
        private AreaViewCreator areaViewCreator;
        private List<GameObject> lockedSpacesAI = new();

        public int MOVES_PER_TURN = 2;
        private const float MOVE_DURATION = 0.25f;
        private const float MOVE_DELAY = 0.1f;

        private static bool _subscribed = false;

        private void Awake()
        {
            areaViewCreator = FindFirstObjectByType<AreaViewCreator>();
        }

        private void OnEnable()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                GameSystem.Instance.OnEnemyTurn += EnemySystem.Instance.ManageEnemiesTurn;
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _subscribed = false;
                GameSystem.Instance.OnEnemyTurn -= EnemySystem.Instance.ManageEnemiesTurn;
            }
        }

        public void OnEnemyTurn()
        {
            Debug.Log("Enemy Moves");
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
                foreach (var space in lockedSpacesAI)
                {
                    if (space != null)
                    {
                        space.transform.DOKill();
                        Destroy(space);
                    }
                }
                lockedSpacesAI.Clear();

                if (gs != null) gs.CompleteTurnAction();
            });
        }

        public IEnumerator WaitEnemyTurn()
        {
            float duration = (MOVE_DURATION + MOVE_DELAY) * MOVES_PER_TURN;
            yield return new WaitForSeconds(duration);
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

                    if (GameSystem.gameEnd)
                        return;
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
                var maskController = player.GetComponent<PlayerMaskController>();
                if (maskController != null && maskController.CurrentMask is InvisibleMaskEffect)
                    continue;

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

                if (diff.y == 0)
                {
                    bool playerInBotHalfMap = target.y <= (areaViewCreator._gridSize+1) / 2;
                    diff.y = playerInBotHalfMap ? 1 : -1;
                }

                var dirY = new Vector2Int(0, System.Math.Sign(diff.y));
                if (IsValidMove(current + dirY)) return dirY;

                if (IsValidMove(current - dirY))
                {
                    LockSpaceAIMovement(current);

                    return -dirY;
                }

                if (IsValidMove(current - dirX))
                {
                    LockSpaceAIMovement(current);
                    return -dirX;
                }
            }
            else
            {
                var dirY = new Vector2Int(0, System.Math.Sign(diff.y));
                if (IsValidMove(current + dirY)) return dirY;

                if (diff.x == 0)
                {
                    bool playerInLeftHalfMap = target.x <= (areaViewCreator._gridSize + 1) / 2;
                    diff.x = playerInLeftHalfMap ? 1 : -1;
                }

                var dirX = new Vector2Int(System.Math.Sign(diff.x), 0);
                if (IsValidMove(current + dirX)) return dirX;

                if (IsValidMove(current - dirX))
                {
                    LockSpaceAIMovement(current);
                    return -dirX;
                }

                if (IsValidMove(current - dirY))
                {
                    LockSpaceAIMovement(current);
                    return -dirY;
                }
            }

            return Vector2Int.zero;
        }

        private void LockSpaceAIMovement(Vector2Int currentCoord)
        {
            GameObject lockSpaceGO = new();
            lockSpaceGO.AddComponent<ObstacleView>();

            GameObject newLockSpaceGO = areaViewCreator.SpawnAt(lockSpaceGO, currentCoord);
            lockedSpacesAI.Add(newLockSpaceGO);
        }

        private bool IsValidMove(Vector2Int coord)
        {
            if (!GridSystem.Instance.TryGetArea(coord, out var area))
                return false;

            return !IsBlockedByObstacle(area) && !IsBlockedByEnemy(area);
        }

        private bool IsBlockedByObstacle(AreaView area)
        {
            if (!area.IsOccupied) return false;

            var obstacle = area.CharacterContainer.GetComponentInChildren<ObstacleView>();
            return obstacle != null;
        }

        private bool IsBlockedByEnemy(AreaView area)
        {
            if (!area.IsOccupied) return false;

            var enemy = area.CharacterContainer.GetComponentInChildren<EnemyView>();
            return enemy != null;
        }
    }
}