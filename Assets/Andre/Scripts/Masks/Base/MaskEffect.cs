using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public abstract class MaskEffect : ScriptableObject
    {
        [SerializeField] private Sprite _maskSprite;
        [SerializeField] private int _duration = 3;

        public Sprite MaskSprite => _maskSprite;
        public int Duration => _duration;
        
        /// <summary>
        /// Called when the mask is picked up (immediate effect). Return the number of extra moves
        /// that should be granted immediately to the picker. Default is 0.
        /// </summary>
        public virtual int OnPickup(GameObject target) { return 0; }

        /// <summary>
        /// Execute immediate (visual/feedback) logic when the mask is triggered via TriggerEffect.
        /// Kept as void for backward-compatibility; use OnPickup() to signal extra-move grants.
        /// </summary>
        public abstract void Execute(GameObject target);

        /// <summary>
        /// Called at the start of the player's turn if the mask is equipped.
        /// </summary>
        public virtual void OnTurnStart(GameObject target) { }
        public virtual void OnRemove(GameObject target) { }

    }
}