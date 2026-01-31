using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Andre.Scripts.Toasts
{
    public class ToastViewUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _iconContainer;

        public void Setup(string message, ToastPresetSO preset)
        {
            // Define o texto
            _messageText.text = message;

            if (preset == null) return;
            
            // Define a cor de fundo
            _backgroundImage.color = preset.BackgroundColor;

            // Define o ícone se houver
            if (preset.ShowIcon && preset.Icon != null)
            {
                _iconContainer.SetActive(true);
                _iconImage.sprite = preset.Icon;
            }
            else
            {
                _iconContainer.SetActive(false);
            }
        }
    }
}