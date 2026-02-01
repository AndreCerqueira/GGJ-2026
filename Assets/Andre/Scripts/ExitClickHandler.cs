using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts
{
    public class ExitClickHandler : MonoBehaviour
    {
        private void OnMouseDown()
        {
            // Procura o componente AreaView nos pais deste objeto
            var area = GetComponentInParent<AreaView>();
            
            if (area != null)
            {
                // Simula o clique na área, permitindo o movimento
                AreaMovementSystem.Instance.OnAreaClicked(area);
            }
        }
    }
}