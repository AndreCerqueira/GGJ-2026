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
        [SerializeField] private GameObject _exitPrefab;

        [Header("Prefabs - Characters")]
        [SerializeField] private GameObject _player1Prefab; // Prefab do Jogador 1
        [SerializeField] private GameObject _player2Prefab; // Prefab do Jogador 2
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Grid Settings")]
        [SerializeField] public int _gridSize = 7;
        [SerializeField] private float _spacing = 1.1f;

        [Header("Spawn Settings")]
        [SerializeField] private int _obstacleCount = 5;
        [SerializeField] private Vector2Int _player1Coord = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int _player2Coord = new Vector2Int(0, 1);
        [SerializeField] private Vector2Int _enemyCoord = new Vector2Int(6, 6);
        private const int MIN_ENEMY_DISTANCE = 4;
        private const int MIN_EXIT_DISTANCE = 6;

        private const float _animDuration = 0.5f;
        private const float _delayStep = 0.05f;

        private HashSet<Vector2Int> usedCoords = new();

        public void Initialize(bool onlySurvivers)
        {
            CreateGrid();

            usedCoords.Clear();
            PlayerView.AllPlayers.Clear();

            SpawnEntities(onlySurvivers);
            SpawnExit();
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

        private void SpawnEntities(bool onlySurvivers)
        {
            // Agora usamos os prefabs específicos para cada jogador
            if (!onlySurvivers || ExitManager.player1Saved)
            {
                ExitManager.player1Saved = false;
                SpawnAt(_player1Prefab, _player1Coord);
                usedCoords.Add(_player1Coord);
            }

            if (!onlySurvivers || ExitManager.player2Saved)
            {
                ExitManager.player2Saved = false;
                SpawnAt(_player2Prefab, _player2Coord);
                usedCoords.Add(_player2Coord);
            }

            EnemySystem.Instance.SpawnEnemies();
        }

        private void SpawnExit()
        {
            Vector2Int exitCoord = PickExitCoord(usedCoords);
            GameObject exit = SpawnAt(_exitPrefab, exitCoord, true);
            exit.transform.parent = exit.transform.parent.parent;
            exit.transform.rotation = _exitPrefab.transform.rotation;
            exit.transform.localScale = _exitPrefab.transform.localScale;
            usedCoords.Add(new(exitCoord.x, exitCoord.y));
        }

        private Vector2Int PickExitCoord(HashSet<Vector2Int> usedCoords)
        {
            const int maxAttempts = 100;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // Pick a random side: 0=left, 1=right, 2=bottom, 3=top
                int side = Random.Range(0, 4);

                Vector2Int coord = side switch
                {
                    0 => new Vector2Int(0, Random.Range(0, _gridSize)),
                    1 => new Vector2Int(_gridSize - 1, Random.Range(0, _gridSize)),
                    2 => new Vector2Int(Random.Range(0, _gridSize), 0),
                    _ => new Vector2Int(Random.Range(0, _gridSize), _gridSize - 1)
                };

                if (usedCoords.Contains(coord))
                {
                    attempts++;
                    continue;
                }

                if (GridDistance(coord, _player1Coord) < MIN_EXIT_DISTANCE)
                {
                    attempts++;
                    continue;
                }

                if (GridDistance(coord, _player2Coord) < MIN_EXIT_DISTANCE)
                {
                    attempts++;
                    continue;
                }

                return coord;
            }

            Debug.LogWarning("Could not find valid exit coord, using fallback.");
            return new Vector2Int(_gridSize - 1, _gridSize - 1);
        }

        public GameObject SpawnNewEnemy()
        {
            PickEnemyCoord(usedCoords);
            GameObject newEnemy = SpawnAt(_enemyPrefab, _enemyCoord);
            usedCoords.Add(new(_enemyCoord.x, _enemyCoord.y));

            return newEnemy;
        }

        private void PickEnemyCoord(HashSet<Vector2Int> usedCoords)
        {
            const int maxAttempts = 100;
            var attempts = 0;

            while (attempts < maxAttempts)
            {
                var coord = new Vector2Int(
                    Random.Range(0, _gridSize),
                    Random.Range(0, _gridSize)
                );

                if (usedCoords.Contains(coord))
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

        public GameObject SpawnAt(GameObject prefab, Vector2Int coord, bool dontAnimate = false)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[AreaViewCreator] Prefab is missing for coord {coord}");
                return null;
            }

            if (!GridSystem.Instance.TryGetArea(coord, out var targetArea)) return null;

            var entity = Instantiate(prefab, targetArea.CharacterContainer);

            if (!dontAnimate)
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