using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class PingPongRotationUI : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 2.0f;
        [SerializeField] private float _rotationAngle = 15f;

        private Vector3 _startRotation;

        private void Start()
        {
            _startRotation = transform.localEulerAngles;
            StartCoroutine(PerformRotation());
        }

        private IEnumerator PerformRotation()
        {
            while (true)
            {
                var newZ = _startRotation.z + Mathf.Sin(Time.time * _rotationSpeed) * _rotationAngle;
                transform.localRotation = Quaternion.Euler(_startRotation.x, _startRotation.y, newZ);
                yield return null;
            }
        }
    }
}