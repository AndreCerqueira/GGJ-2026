using Andre.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace Andre.Scripts
{
    public class ExitTransparency : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Lista de renderers a afetar. Se vazio, procura automaticamente nos filhos.")]
        [SerializeField] private List<Renderer> _renderers;
        
        [SerializeField] private float _startFadeDistance = 3f;
        [SerializeField] private float _maxFadeDistance = 8f;
        
        [Range(0f, 1f)]
        [SerializeField] private float _minAlpha = 0.0f;

        // Lista para guardar as instâncias dos materiais e evitar chamadas ao .material no Update
        private List<Material> _instantiatedMaterials = new List<Material>();
        private const float ALPHA_THRESHOLD = 0.01f;

        private void Awake()
        {
            // Se a lista não for preenchida no Inspector, procura em todos os filhos
            if (_renderers == null || _renderers.Count == 0)
            {
                _renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
            }

            // Armazena os materiais de cada renderer para modificar
            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    _instantiatedMaterials.Add(renderer.material);
                }
            }
        }

        private void Update()
        {
            UpdateTransparency();
        }

        private void UpdateTransparency()
        {
            if (_instantiatedMaterials.Count == 0 || PlayerView.AllPlayers.Count == 0) return;

            var closestDistance = float.MaxValue;

            // Calcula a distância para o jogador mais próximo
            foreach (var player in PlayerView.AllPlayers)
            {
                if (player == null) continue;

                var dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                }
            }

            // Calcula o Alpha alvo com base na distância
            var t = Mathf.InverseLerp(_startFadeDistance, _maxFadeDistance, closestDistance);
            var targetAlpha = Mathf.Lerp(1f, _minAlpha, t);

            // Aplica o novo Alpha a todos os materiais guardados
            foreach (var mat in _instantiatedMaterials)
            {
                if (mat == null) continue;

                var currentColor = mat.color;

                // Só aplica se a mudança for significativa (otimização)
                if (Mathf.Abs(currentColor.a - targetAlpha) > ALPHA_THRESHOLD)
                {
                    currentColor.a = targetAlpha;
                    mat.color = currentColor;
                }
            }
        }
    }
}