using Andre.Scripts.Systems;
using UnityEngine;
using Project.Runtime.Scripts.General; // <--- ADICIONA ISTO (Namespace do CursorManager)

namespace Andre.Scripts
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Light _playerLight;
        public Light PlayerLight => _playerLight;
        
        public static System.Collections.Generic.List<PlayerView> AllPlayers { get; } = new System.Collections.Generic.List<PlayerView>();

        private void OnEnable()
        {
            if (!AllPlayers.Contains(this)) AllPlayers.Add(this);
        }

        private void OnDisable()
        {
            AllPlayers.Remove(this);
            // Segurança: Se o jogador morrer/desaparecer enquanto o rato está em cima, reseta o cursor
            if (CursorManager.Instance != null) CursorManager.Instance.SetDefaultCursor();
        }

        private void OnMouseDown()
        {
            AreaMovementSystem.Instance.SelectPlayer(this);
        }

        // --- ADICIONA ESTES MÉTODOS ---

        private void OnMouseEnter()
        {
            // Quando o rato entra no jogador, muda para a "mãozinha"
            if (CursorManager.Instance != null) 
                CursorManager.Instance.SetInteractCursor();
        }

        private void OnMouseExit()
        {
            // Quando o rato sai, volta ao normal
            if (CursorManager.Instance != null) 
                CursorManager.Instance.SetDefaultCursor();
        }
    }
}