using UnityEngine;

namespace Project.Runtime.Scripts.Utils
{
    public class LookAtCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _lookAtCamera = true;
        [SerializeField] private bool _sameDirectionAsCamera = false;
        [SerializeField] private bool _lockX = false;
        [SerializeField] private bool _lockY = false;
        [SerializeField] private bool _lockZ = false;

        [SerializeField] private Transform _cameraTransform;
        
        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (!_lookAtCamera || _cameraTransform == null)
                return;

            if (_sameDirectionAsCamera)
                transform.rotation = Quaternion.LookRotation(_cameraTransform.forward, Vector3.up);
            else
                transform.LookAt(_cameraTransform);

            Vector3 rotation = transform.eulerAngles;
            if (_lockX) rotation.x = 0f;
            if (_lockY) rotation.y = 0f;
            if (_lockZ) rotation.z = 0f;
            transform.eulerAngles = rotation;
        }
    }
}
