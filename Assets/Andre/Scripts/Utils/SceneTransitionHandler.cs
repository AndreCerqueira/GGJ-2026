using MoreMountains.Feedbacks;
using UnityEngine;

namespace Project.Runtime.Scripts.Utils
{
    public class SceneTransitionHandler : Singleton<SceneTransitionHandler>
    {
        [SerializeField] private MMF_Player _transitionFeedback;

        public void LoadScene(string sceneName)
        {
            var loadSceneFeedback = _transitionFeedback.GetFeedbackOfType<MMF_LoadScene>();

            if (loadSceneFeedback != null)
            {
                loadSceneFeedback.DestinationSceneName = sceneName;
                _transitionFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning("[SceneTransitionHandler] MMF_LoadScene feedback not found. Loading directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
    }
}