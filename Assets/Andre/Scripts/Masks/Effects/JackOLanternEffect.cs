using Andre.Scripts.Masks.Base;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewJackOLanternEffect", menuName = "Effects/Jack O' Lantern Effect")]
    public class JackOLanternEffect : MaskEffect
    {
        [Header("Light Settings")]
        [Tooltip("Valor a adicionar à intensidade atual da luz.")]
        [SerializeField] private float _intensityBoost = 5f;

        public override void Execute(GameObject target)
        {
            // O 'target' no Execute vem do Spawner (é a AreaView)
            var varArea = target.GetComponent<AreaView>();
            if (varArea == null) return;

            var varPlayer = varArea.CharacterContainer.GetComponentInChildren<PlayerView>();
            
            if (varPlayer != null && varPlayer.PlayerLight != null)
            {
                varPlayer.PlayerLight.intensity += _intensityBoost;
            }
        }

        public override void OnRemove(GameObject target)
        {
            // O 'target' no OnRemove vem do PlayerMaskController (é o Player)
            var varPlayer = target.GetComponent<PlayerView>();

            if (varPlayer != null && varPlayer.PlayerLight != null)
            {
                varPlayer.PlayerLight.intensity -= _intensityBoost;
            }
        }
    }
}