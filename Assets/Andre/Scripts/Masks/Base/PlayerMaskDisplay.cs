using UnityEngine;

namespace Andre.Scripts
{
    public class PlayerMaskDisplay : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        public void SetMask(Sprite maskSprite)
        {
            if (_renderer == null) return;
            
            _renderer.sprite = maskSprite;
            _renderer.enabled = maskSprite != null;
        }
    }
}