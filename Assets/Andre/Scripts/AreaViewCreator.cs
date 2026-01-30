using UnityEngine;
using DG.Tweening;

namespace Game.World
{
    public class AreaViewCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private int _gridSize = 7;
        [SerializeField] private float _spacing = 1.1f;
        [SerializeField] private float _animationDuration = 0.5f;

        private const float _defaultDelay = 0.05f;
        private const float _scaleStrength = 0.2f;

        private void Start()
        {
            CreateGrid();
        }

        public void CreateGrid()
        {
            for (var x = 0; x < _gridSize; x++)
            {
                for (var z = 0; z < _gridSize; z++)
                {
                    var position = new Vector3(x * _spacing, 0, z * _spacing);
                    var cube = Instantiate(_cubePrefab, position, Quaternion.identity, transform);
                    
                    AnimateCube(cube.transform, x, z);
                }
            }
        }

        private void AnimateCube(Transform target, int x, int z)
        {
            var delay = (x + z) * _defaultDelay;
            
            target.localScale = Vector3.zero;
            target.DOScale(Vector3.one, _animationDuration)
                  .SetDelay(delay)
                  .SetEase(Ease.OutBack);

            target.DOMoveY(0, _animationDuration)
                  .From(Vector3.down.y * _scaleStrength)
                  .SetDelay(delay);
        }
    }
}