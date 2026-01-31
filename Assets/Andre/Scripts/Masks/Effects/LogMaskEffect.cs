using Andre.Scripts.Masks.Base;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewLogEffect", menuName = "Effects/Log Effect")]
    public class LogMaskEffect : MaskEffect
    {
        [SerializeField] private string _message = "Mask effect triggered!";

        public override void Execute(GameObject target)
        {
            Debug.Log($"[Effect System] {_message} | Target: {target.name}");
        }
    }
}