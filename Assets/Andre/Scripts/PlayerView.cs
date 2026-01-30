using UnityEngine;

namespace Andre.Scripts
{
    public class PlayerView : MonoBehaviour
    {
        private void OnMouseDown()
        {
            AreaMovementSystem.Instance.SelectPlayer(this);
        }
    }
}