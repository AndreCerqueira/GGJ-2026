using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public abstract class MaskEffect : ScriptableObject
    {
        [SerializeField] private Sprite _maskSprite;
        [SerializeField] private int _duration = 3;

        public Sprite MaskSprite => _maskSprite;
        public int Duration => _duration;
        
        public abstract void Execute(GameObject target);
    }
}