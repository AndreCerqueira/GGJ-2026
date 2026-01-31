using System.Collections.Generic;
using Andre.Scripts.Masks;
using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts
{
    public class MaskSpawnerSystem : MonoBehaviour
    {
        public static MaskSpawnerSystem Instance { get; private set; }

        [SerializeField] private GameObject _maskPrefab;
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
                MaskEffectSystem.Instance.TriggerEffect(varMaskInstance.Effect, area.gameObject);
            }

            _activeMasks.Remove(mask);
            Destroy(mask);
            SpawnNewMask();
        }

        private void SpawnNewMask()
        {
            var varAvailableAreas = GetValidSpawnAreas();
            if (varAvailableAreas.Count == 0) return;

            var varRandomIndex = Random.Range(0, varAvailableAreas.Count);
            var varTargetArea = varAvailableAreas[varRandomIndex];

            var varMask = Instantiate(_maskPrefab, varTargetArea.MaskContainer);
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