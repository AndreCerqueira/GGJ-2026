using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public abstract class MaskEffect : ScriptableObject
    {
        [SerializeField] private Sprite _maskSprite;
        public Sprite MaskSprite => _maskSprite;
        
        public abstract void Execute(GameObject target);
    }
}