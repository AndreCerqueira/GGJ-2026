using System.Collections.Generic;
using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewTeleportEffect", menuName = "Effects/Teleport Effect")]
    public class TeleportMaskEffect : MaskEffect
    {
        [Header("Feedback")]
        [SerializeField] private ToastPresetSO _feedbackPreset;
        [SerializeField] private string _message = "Teleported!";

        public override void Execute(GameObject target)
        {
            // O 'target' recebido é o GameObject da AreaView onde a máscara estava
            var varCurrentArea = target.GetComponent<AreaView>();
            if (varCurrentArea == null) return;

            // Tenta encontrar o Player que acabou de entrar nesta área (está no CharacterContainer)
            var varPlayer = varCurrentArea.CharacterContainer.GetComponentInChildren<PlayerView>();
            
            // Se não encontrar PlayerView, aborta (para não teleportar inimigos acidentalmente)
            if (varPlayer == null) return;

            // Feedback visual (Toast)
            if (ToastSystem.Instance != null)
            {
                ToastSystem.Instance.Show(_message, _feedbackPreset);
            }

            TeleportToRandomSpot(varPlayer.transform, varCurrentArea);
        }

        private void TeleportToRandomSpot(Transform playerTransform, AreaView currentArea)
        {
            if (GridSystem.Instance == null) return;

            var varValidSpots = new List<AreaView>();

            // 1. Procurar todas as áreas válidas no Grid
            foreach (var varArea in GridSystem.Instance.GetAllAreas())
            {
                // Regras: não ser a área atual, não ter obstáculos, não ter ninguém
                if (varArea != currentArea && !varArea.HasObstacle && !varArea.IsOccupied)
                {
                    varValidSpots.Add(varArea);
                }
            }

            // 2. Escolher uma aleatória
            if (varValidSpots.Count > 0)
            {
                var varRandomIndex = Random.Range(0, varValidSpots.Count);
                var varTargetArea = varValidSpots[varRandomIndex];

                // 3. Teleportar
                // Ao mudar o parent, o sistema de jogo saberá que o jogador está na nova área
                playerTransform.SetParent(varTargetArea.CharacterContainer);
                
                // Resetar a posição local para (0,0,0) para ficar centralizado no novo tile
                playerTransform.localPosition = Vector3.zero;

                Debug.Log($"[TeleportEffect] Teleportado de {currentArea.name} para {varTargetArea.name}");
            }
            else
            {
                Debug.LogWarning("[TeleportEffect] Não há lugares livres para teleportar!");
            }
        }
    }
}