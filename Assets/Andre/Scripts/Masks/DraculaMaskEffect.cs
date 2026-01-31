using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using UnityEngine;
using System.Collections.Generic;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using DG.Tweening;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewDraculaEffect", menuName = "Effects/Dracula Effect")]
    public class DraculaMaskEffect : MaskEffect
    {
        [SerializeField] private ToastPresetSO _effectPreset;
        [SerializeField] private string _message = "Dracula's Curse";
        
        private const float MOVE_DURATION = 0.3f;
        private const int MAX_REPETITIONS = 3;

        public override void Execute(GameObject target)
        {
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _effectPreset);
            }
            
            var varAllCharacters = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var varDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var varChar in varAllCharacters)
            {
                var varIsPlayer = varChar is PlayerView;
                var varIsEnemy = varChar is EnemyView;

                if (!varIsPlayer && !varIsEnemy) continue;

                var varRepetitions = Random.Range(1, MAX_REPETITIONS + 1);
                if (varRepetitions == 0) continue;

                ApplyMultiStepMovement(varChar.transform, varRepetitions, varDirections);
            }
        }

        private void ApplyMultiStepMovement(Transform character, int steps, Vector2Int[] directions)
        {
            var varSequence = DOTween.Sequence();

            for (var i = 0; i < steps; i++)
            {
                varSequence.AppendCallback(() => 
                {
                    var varCurrentArea = character.GetComponentInParent<AreaView>();
                    if (varCurrentArea == null) return;

                    var varValidAreas = new List<AreaView>();
                    foreach (var varDir in directions)
                    {
                        var varTargetCoord = varCurrentArea.Coordinate + varDir;
                        if (GridSystem.Instance.TryGetArea(varTargetCoord, out var varArea))
                        {
                            if (!varArea.HasObstacle && !varArea.IsOccupied)
                            {
                                varValidAreas.Add(varArea);
                            }
                        }
                    }

                    if (varValidAreas.Count > 0)
                    {
                        var varRandomArea = varValidAreas[Random.Range(0, varValidAreas.Count)];
                        character.SetParent(varRandomArea.CharacterContainer);
                        character.DOLocalMove(Vector3.zero, MOVE_DURATION).SetEase(Ease.OutQuad);
                    }
                });
                
                varSequence.AppendInterval(MOVE_DURATION + 0.05f);
            }
        }
    }
}