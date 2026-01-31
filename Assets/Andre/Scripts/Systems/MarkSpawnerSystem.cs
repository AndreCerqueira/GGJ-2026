using System.Collections.Generic;
using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts
{
    public class MaskSpawnerSystem : MonoBehaviour
    {
        public static MaskSpawnerSystem Instance { get; private set; }

        [SerializeField] private List<GameObject> _maskPrefabs;
        [SerializeField] private int _maxMasks = 3;

        private List<GameObject> _activeMasks = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
        }

        public void SpawnInitialMasks()
        {
            for (var i = 0; i < _maxMasks; i++)
            {
                SpawnNewMask();
            }
        }

        public void OnMaskPicked(GameObject mask, AreaView area)
        {
            var varMaskInstance = mask.GetComponent<MaskInstance>();
    
            if (varMaskInstance != null && varMaskInstance.Effect != null)
            {
                UpdatePlayerVisual(area, varMaskInstance.Effect.MaskSprite);
                MaskEffectSystem.Instance.TriggerEffect(varMaskInstance.Effect, area.gameObject);
            }

            _activeMasks.Remove(mask);
            Destroy(mask);
            SpawnNewMask();
        }

        private void UpdatePlayerVisual(AreaView area, Sprite maskSprite)
        {
            var varPlayer = area.CharacterContainer.GetComponentInChildren<PlayerView>();
            if (varPlayer != null)
            {
                var varDisplay = varPlayer.GetComponentInChildren<PlayerMaskDisplay>();
                if (varDisplay != null)
                {
                    varDisplay.SetMask(maskSprite);
                }
            }
        }

        private void SpawnNewMask()
        {
            if (_maskPrefabs == null || _maskPrefabs.Count == 0) return;

            var varAvailableAreas = GetValidSpawnAreas();
            if (varAvailableAreas.Count == 0) return;

            var varRandomAreaIndex = Random.Range(0, varAvailableAreas.Count);
            var varTargetArea = varAvailableAreas[varRandomAreaIndex];

            var varRandomPrefabIndex = Random.Range(0, _maskPrefabs.Count);
            var varSelectedPrefab = _maskPrefabs[varRandomPrefabIndex];

            var varMask = Instantiate(varSelectedPrefab, varTargetArea.MaskContainer);
            _activeMasks.Add(varMask);
        }

        private List<AreaView> GetValidSpawnAreas()
        {
            var varValidAreas = new List<AreaView>();
            var varForbiddenCoords = GetForbiddenCoordinates();

            foreach (var varArea in GridSystem.Instance.GetAllAreas())
            {
                if (varArea.HasMask || varArea.IsOccupied || varForbiddenCoords.Contains(varArea.Coordinate)) 
                    continue;
                
                varValidAreas.Add(varArea);
            }

            return varValidAreas;
        }

        private HashSet<Vector2Int> GetForbiddenCoordinates()
        {
            var varForbidden = new HashSet<Vector2Int>();
            var varDirections = new Vector2Int[] 
            { 
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            foreach (var varArea in GridSystem.Instance.GetAllAreas())
            {
                if (varArea.IsOccupied || varArea.HasMask)
                {
                    foreach (var varDir in varDirections)
                    {
                        varForbidden.Add(varArea.Coordinate + varDir);
                    }
                }
            }
            
            return varForbidden;
        }
    }
}