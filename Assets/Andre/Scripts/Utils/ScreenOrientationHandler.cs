using UnityEngine;

namespace Project.Runtime.Scripts.Utils
{
    public class ScreenOrientationHandler : MonoBehaviour
    {
        [SerializeField] private ScreenOrientation _screenOrientation;

        private void Start()
        {
            if (_screenOrientation == ScreenOrientation.AutoRotation)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }

            Screen.orientation = _screenOrientation;
        }
    }
}