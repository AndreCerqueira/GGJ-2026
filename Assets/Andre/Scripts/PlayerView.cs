using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts
{
    public class PlayerView : MonoBehaviour
    {
        // Static registry of players to make checks easier at runtime
        public static System.Collections.Generic.List<PlayerView> AllPlayers { get; } = new System.Collections.Generic.List<PlayerView>();

        private void OnEnable()
        {
            if (!AllPlayers.Contains(this)) AllPlayers.Add(this);
        }

        private void OnDisable()
        {
            AllPlayers.Remove(this);
        }

        private void OnMouseDown()
        {
            AreaMovementSystem.Instance.SelectPlayer(this);
        }
    }
}