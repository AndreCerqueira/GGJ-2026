using Andre.Scripts.Systems;
using DG.Tweening;
using UnityEngine;

namespace Andre.Scripts
{
    public class AreaViewCreator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private AreaView _areaPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Grid Settings")]
        [SerializeField] private int _gridSize = 7;
        [SerializeField] private float _spacing = 1.1f;

        [Header("Spawn Positions (Coordinates)")]
        [SerializeField] private Vector2Int _player1Coord = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int _player2Coord = new Vector2Int(0, 1);
        [SerializeField] private Vector2Int _enemyCoord = new Vector2Int(6, 6);

        private const float _animDuration = 0.5f;
        private const float _delayStep = 0.05f;

        public void Initialize()
        {
            CreateGrid(); 
            SpawnEntities();
            MaskSpawnerSystem.Instance.SpawnInitialMasks();
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
            SpawnAt(_playerPrefab, _player1Coord);
            SpawnAt(_playerPrefab, _player2Coord);
            SpawnAt(_enemyPrefab, _enemyCoord);
        }

        private void SpawnAt(GameObject prefab, Vector2Int coord)
        {
            if (!GridSystem.Instance.TryGetArea(coord, out var targetArea)) return;

            var entity = Instantiate(prefab, targetArea.CharacterContainer);
            
            AnimateSpawn(entity, coord.x, coord.y);
        }

        private void AnimateSpawn(GameObject go, int x, int z)
        {
            var varDelay = (x + z) * _delayStep;
            go.transform.localScale = Vector3.zero;
            go.transform.DOScale(Vector3.one, _animDuration)
                .SetDelay(varDelay)
                .SetEase(Ease.OutBack);
        }
    }
}