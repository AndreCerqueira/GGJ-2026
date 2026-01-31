using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public class MaskInstance : MonoBehaviour
    {
        [SerializeField] private MaskEffect _effect;

        public MaskEffect Effect => _effect;
    }
}