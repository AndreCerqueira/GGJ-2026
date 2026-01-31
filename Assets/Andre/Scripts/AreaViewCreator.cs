using System.Collections.Generic;
using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class AreaViewCreator : MonoBehaviour
    {
        [Header("Prefabs - Environment")]
        [SerializeField] private AreaView _areaPrefab;
        [SerializeField] private GameObject _obstaclePrefab;

        [Header("Prefabs - Characters")]
        [SerializeField] private GameObject _player1Prefab; // Prefab do Jogador 1
        [SerializeField] private GameObject _player2Prefab; // Prefab do Jogador 2
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Grid Settings")]
        [SerializeField] private int _gridSize = 7;
        [SerializeField] private float _spacing = 1.1f;

        [Header("Spawn Settings")]
        [SerializeField] private int _obstacleCount = 5;
        [SerializeField] private Vector2Int _player1Coord = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int _player2Coord = new Vector2Int(0, 1);
        [SerializeField] private Vector2Int _enemyCoord = new Vector2Int(6, 6);
        private const int MIN_ENEMY_DISTANCE = 4;

        private const float _animDuration = 0.5f;
        private const float _delayStep = 0.05f;

        public void Initialize()
        {
            CreateGrid();
            SpawnEntities();
            SpawnObstacles();
            MaskSpawnerSystem.Instance.SpawnInitialMasks();

            var gs = GameSystem.GetOrFindInstance();
            if (gs == null)
            {
                GameSystem.RegisterOnInitialized(() => GameSystem.Instance.StartGame());
            }
            else
            {
                gs.StartGame();
            }
        }

        private void CreateGrid()
        {
            for (var x = 0; x < _gridSize; x++)
            {
                for (var z = 0; z < _gridSize; z++)
                {
                    var coord = new Vector2Int(x, z);
                    var worldPos = new Vector3(x * _spacing, 0, z * _spacing);

                    var area = Instantiate(_areaPrefab, worldPos, Quaternion.identity, transform);
                    area.name = $"Area_{x}_{z}";
                    area.Setup(coord);

                    GridSystem.Instance.RegisterArea(coord, area);
                    AnimateSpawn(area.gameObject, x, z);
                }
            }
        }

        private void SpawnEntities()
        {
            // Agora usamos os prefabs específicos para cada jogador
            SpawnAt(_player1Prefab, _player1Coord);
            SpawnAt(_player2Prefab, _player2Coord);

            PickEnemyCoord();
            SpawnAt(_enemyPrefab, _enemyCoord);
        }

        private void PickEnemyCoord()
        {
            const int maxAttempts = 100;
            var attempts = 0;

            while (attempts < maxAttempts)
            {
                var coord = new Vector2Int(
                    Random.Range(0, _gridSize),
                    Random.Range(0, _gridSize)
                );

                if (coord == _player1Coord || coord == _player2Coord)
                {
                    attempts++;
                    continue;
                }

                if (GridDistance(coord, _player1Coord) < MIN_ENEMY_DISTANCE)
                {
                    attempts++;
                    continue;
                }

                if (GridDistance(coord, _player2Coord) < MIN_ENEMY_DISTANCE)
                {
                    attempts++;
                    continue;
                }

                _enemyCoord = coord;
                return;
            }

            _enemyCoord = new Vector2Int(_gridSize - 1, _gridSize - 1);
        }

        private int GridDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private void SpawnObstacles()
        {
            var usedCoords = new HashSet<Vector2Int> { _player1Coord, _player2Coord, _enemyCoord };
            var attempts = 0;
            var spawned = 0;

            while (spawned < _obstacleCount && attempts < 100)
            {
                var x = Random.Range(0, _gridSize);
                var z = Random.Range(0, _gridSize);
                var coord = new Vector2Int(x, z);

                if (!usedCoords.Contains(coord))
                {
                    SpawnAt(_obstaclePrefab, coord);
                    usedCoords.Add(coord);
                    spawned++;
                }

                attempts++;
            }
        }

        public GameObject SpawnAt(GameObject prefab, Vector2Int coord)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[AreaViewCreator] Prefab is missing for coord {coord}");
                return null;
            }

            if (!GridSystem.Instance.TryGetArea(coord, out var targetArea)) return null;

            var entity = Instantiate(prefab, targetArea.CharacterContainer);
            AnimateSpawn(entity, coord.x, coord.y);

            return entity;
        }

        private void AnimateSpawn(GameObject go, int x, int z)
        {
            var delay = (x + z) * _delayStep;
            go.transform.localScale = Vector3.zero;
            go.transform.DOScale(Vector3.one, _animDuration)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);
        }
    }
}