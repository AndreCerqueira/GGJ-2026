using System.Collections.Generic;
using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public class MaskEffectSystem : MonoBehaviour
    {
        public static MaskEffectSystem Instance { get; private set; }

        private Queue<MaskEffect> _effectQueue = new Queue<MaskEffect>();
        private bool _isProcessing;

        private void Awake()
        {
            Instance = this;
        }

        public void TriggerEffect(MaskEffect effect, GameObject target)
        {
            if (effect == null) return;
            
            effect.Execute(target);
        }
        
        
    }
}