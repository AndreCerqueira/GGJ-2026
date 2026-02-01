using Andre.Scripts.Masks.Base;
using Andre.Scripts.UI;
using Andre.Scripts.Toasts;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewPlagueDoctorEffect", menuName = "Effects/Plague Doctor Effect")]
    public class PlagueDoctorMaskEffect : MaskEffect
    {
        [SerializeField] private ToastPresetSO _effectPreset;
        [SerializeField] private string _message = "Heroes never die!";

        public override void Execute(GameObject target)
        {
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _effectPreset);
            }

            var tombstones = FindObjectsByType<Tombstone>(FindObjectsSortMode.None);
            
            // LOG DE DEBUG 1: Quantas lápides achou?
            Debug.Log($"[PlagueDoctor] Encontradas {tombstones.Length} lápides na cena.");

            foreach (var tombstone in tombstones)
            {
                if (tombstone.CanRespawn)
                {
                    tombstone.Respawn();
                }
                else
                {
                    // LOG DE DEBUG 2: Avisa se a lápide está defeituosa
                    Debug.LogWarning($"[PlagueDoctor] Lápide encontrada em {tombstone.transform.position}, mas não tem 'PlayerPrefab'. Verifica o HealthSystem do jogador!");
                }
            }
        }
    }
}