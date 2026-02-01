using System.Collections;
using System.Collections.Generic;
using Andre.Scripts.Masks;
using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class EnemyView : MonoBehaviour
    {
        [Header("Transparency Settings")]
        [Tooltip("Referência para o Sprite do inimigo.")]
        [SerializeField] private SpriteRenderer _renderer;
        
        [Tooltip("Distância a partir da qual o inimigo começa a ficar transparente.")]
        [SerializeField] private float _startFadeDistance = 3f;
        
        [Tooltip("Distância onde o inimigo atinge a transparência máxima.")]
        [SerializeField] private float _maxFadeDistance = 8f;
        
        [Tooltip("Valor mínimo de Alpha (0 = invisível, 1 = visível).")]
        [Range(0f, 1f)]
        [SerializeField] private float _minAlpha = 0.2f;

        // Variável para guardar a referência do material instanciado
        private Material _instantiatedMaterial;

        private AreaViewCreator areaViewCreator;
        private List<GameObject> lockedSpacesAI = new();

        public int MOVES_PER_TURN = 2;
        private const float MOVE_DURATION = 0.25f;
        private const float MOVE_DELAY = 0.1f;

        private static bool _subscribed = false;

        private void Awake()
        {
            areaViewCreator = FindFirstObjectByType<AreaViewCreator>();
            
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }

            // Ao acessar .material, o Unity cria uma cópia única para este objeto.
            // Guardamos a referência para alterar a cor no Update sem criar lixo de memória.
            if (_renderer != null)
            {
                _instantiatedMaterial = _renderer.material;
            }
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

        private void Update()
        {
            UpdateTransparency();
        }

        private void UpdateTransparency()
        {
            // Se não houver material instanciado ou jogadores, não faz nada
            if (_instantiatedMaterial == null || PlayerView.AllPlayers.Count == 0) return;

            float closestDistance = float.MaxValue;

            // 1. Encontra a distância para o jogador mais próximo
            foreach (var player in PlayerView.AllPlayers)
            {
                if (player == null) continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                }
            }

            // 2. Calcula o Alpha baseado na distância
            float t = Mathf.InverseLerp(_startFadeDistance, _maxFadeDistance, closestDistance);
            float targetAlpha = Mathf.Lerp(1f, _minAlpha, t);

            // 3. Aplica a cor DIRETAMENTE no material instanciado
            Color currentColor = _instantiatedMaterial.color;
            
            // Só aplica se houver mudança significativa para poupar processamento
            if (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
            {
                currentColor.a = targetAlpha;
                _instantiatedMaterial.color = currentColor;
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