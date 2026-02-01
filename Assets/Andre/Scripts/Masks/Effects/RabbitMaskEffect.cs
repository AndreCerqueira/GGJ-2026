using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts.Masks
{
    [CreateAssetMenu(fileName = "NewRabbitEffect", menuName = "Effects/Rabbit Effect")]
    public class RabbitMaskEffect : MaskEffect
    {
        [SerializeField] private int _extraMoves = 2;

        public override void Execute(GameObject target)
        {
        }

        public override void OnTurnStart(GameObject target)
        {
            if (AreaMovementSystem.Instance != null)
            {
                AreaMovementSystem.Instance.playerMoves += _extraMoves;
            }
        }
    }
}