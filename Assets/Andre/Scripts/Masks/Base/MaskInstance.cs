using System;
using UnityEngine;

namespace Andre.Scripts.Masks.Base
{
    public class MaskInstance : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private MaskEffect _effect;
        public MaskEffect Effect => _effect;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Setup(MaskEffect effect)
        {
            _effect = effect;
            _spriteRenderer.sprite = _effect.MaskSprite;
        }
    }
}