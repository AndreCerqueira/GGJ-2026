using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Runtime.Scripts
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 _rotationAxis;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private bool _useLocalSpace;
    
        private void Update()
        {
            if (Time.timeScale == 0f)
                return;
            
            RotateObject();
        }
    
        private void RotateObject()
        {
            var rotation = _rotationAxis.normalized * _rotationSpeed * Time.deltaTime;

            transform.Rotate(rotation);
        }
    }
}