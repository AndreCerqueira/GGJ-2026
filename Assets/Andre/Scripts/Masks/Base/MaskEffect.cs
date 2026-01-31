using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public abstract class MaskEffect : ScriptableObject
    {
        public abstract void Execute(GameObject target);
    }
}