using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.Effects
{
    public class AutoZRotation : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 90f;
        private Coroutine _rotationCoroutine;

        private void OnEnable()
        {
            _rotationCoroutine = StartCoroutine(RotateForever());
        }

        private void OnDisable()
        {
            if (_rotationCoroutine != null)
                StopCoroutine(_rotationCoroutine);
        }

        private IEnumerator RotateForever()
        {
            while (true)
            {
                transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime, Space.Self);
                yield return null;
            }
        }
    }
}
