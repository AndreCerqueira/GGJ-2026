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
            // O 'target' aqui é o GameObject da AreaView onde a máscara foi apanhada
            var varArea = target.GetComponent<AreaView>();
            if (varArea == null) return;

            var varPlayer = varArea.CharacterContainer.GetComponentInChildren<PlayerView>();
            
            if (varPlayer != null && varPlayer.PlayerLight != null)
            {
                varPlayer.PlayerLight.intensity += _intensityBoost;
            }
        }
    }
}