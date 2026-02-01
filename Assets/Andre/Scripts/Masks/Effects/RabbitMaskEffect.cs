using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewRabbitEffect", menuName = "Effects/Rabbit Effect")]
    public class RabbitMaskEffect : MaskEffect
    {
        [SerializeField] private ToastPresetSO _effectPreset;
        [SerializeField] private string _message = "";
        
        [SerializeField] private int _extraMoves = 2;

        public override void Execute(GameObject target)
        {
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _effectPreset);
            }
        }

        // Grant extra moves immediately when the mask is picked up.
        public override int OnPickup(GameObject target)
        {
            // Show feedback immediately as well
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _effectPreset);
            }
            return _extraMoves;
        }

        public override void OnTurnStart(GameObject target)
        {
            if (AreaMovementSystem.Instance != null)
            {
                AreaMovementSystem.Instance.playerMoves += _extraMoves;
            }
        }

        public override void OnRemove(GameObject target)
        {
        }
    }
}