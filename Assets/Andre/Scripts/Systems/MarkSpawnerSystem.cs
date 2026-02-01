using System.Collections.Generic;
using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using Unity.VisualScripting;
using UnityEngine;

namespace Andre.Scripts
{
    public class MaskSpawnerSystem : MonoBehaviour
    {
        public static MaskSpawnerSystem Instance { get; private set; }
        
        [SerializeField] private MaskInstance _maskInstancePrefab;

        [SerializeField] private List<MaskEffect> _maskEffects;
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

        public void OnMaskPicked(GameObject maskObject, AreaView area)
        {
            var varMaskInstance = maskObject.GetComponent<MaskInstance>();
            var varPlayer = area.CharacterContainer.GetComponentInChildren<PlayerMaskController>();
    
            if (varMaskInstance != null && varMaskInstance.Effect != null)
            {
                Debug.Log($"[MaskSpawnerSystem] Player picked up mask with effect: {varMaskInstance.Effect.name}");
                // Aplica o efeito imediato
                MaskEffectSystem.Instance.TriggerEffect(varMaskInstance.Effect, area.gameObject);

                // If the mask grants immediate extra moves on pickup, apply them now so the player can use them.
                try
                {
                    var extra = varMaskInstance.Effect.OnPickup(area.gameObject);
                    if (extra != 0 && AreaMovementSystem.Instance != null)
                    {
                        AreaMovementSystem.Instance.playerMoves += extra;
                        Debug.Log($"[MaskSpawnerSystem] Applied {extra} immediate extra moves to player.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[MaskSpawnerSystem] Exception when checking OnPickup: {ex}");
                }

                // Equipa a máscara no jogador (lógica de duração)
                if (varPlayer != null)
                {
                    varPlayer.EquipMask(varMaskInstance.Effect);
                }
            }

            _activeMasks.Remove(maskObject);
            Destroy(maskObject);
            SpawnNewMask();
        }

    public void SpawnNewMask()
        {
            if (_maskEffects == null || _maskEffects.Count == 0) return;

            var varAvailableAreas = GetValidSpawnAreas();
            if (varAvailableAreas.Count == 0) return;

            var varRandomAreaIndex = Random.Range(0, varAvailableAreas.Count);
            var varTargetArea = varAvailableAreas[varRandomAreaIndex];

            var varRandomEffectIndex = Random.Range(0, _maskEffects.Count);
            var varSelectedEffect = _maskEffects[varRandomEffectIndex];

            var varMask = Instantiate(_maskInstancePrefab, varTargetArea.MaskContainer);
            varMask.Setup(varSelectedEffect);
            
            _activeMasks.Add(varMask.gameObject);
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