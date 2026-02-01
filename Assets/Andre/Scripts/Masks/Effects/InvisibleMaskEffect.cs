using Andre.Scripts.Masks.Base;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewInvisibleEffect", menuName = "Effects/Invisible Effect")]
    public class InvisibleMaskEffect : MaskEffect
    {
        [SerializeField] private ToastPresetSO _effectPreset;
        [SerializeField] private string _message = "";
        
        public override void Execute(GameObject target)
        {
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _effectPreset);
            }
        }
    }
}