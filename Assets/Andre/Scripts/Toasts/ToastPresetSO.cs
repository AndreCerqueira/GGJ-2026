using UnityEngine;

namespace Andre.Scripts.Toasts
{
    [CreateAssetMenu(fileName = "New Toast Preset", menuName = "UI/Toast Preset")]
    public class ToastPresetSO : ScriptableObject
    {
        [Header("Appearance")]
        public Color BackgroundColor = new Color(0, 0, 0, 0.9f);
        public Sprite Icon;
        public bool ShowIcon = true;

        [Header("Settings")]
        public float DisplayDuration = 2f;
    }
}