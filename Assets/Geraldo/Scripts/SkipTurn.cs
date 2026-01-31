using UnityEngine;

public class SkipTurn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("[SkipTurn] Start() called - will attempt to skip turn next frame");
        // Delay one frame so GameSystem.Start() can run first and initialize state.
        StartCoroutine(DelayedSkip());
    }

    private System.Collections.IEnumerator DelayedSkip()
    {
        yield return null; // wait one frame

        Debug.Log("[SkipTurn] DelayedSkip() running");
        var gs = GameSystem.GetOrFindInstance();
        if (gs == null)
        {
            Debug.LogError("[SkipTurn] No GameSystem found in scene. EnemyTurn not called.");
            yield break;
        }

        Debug.Log("[SkipTurn] Calling GameSystem.Instance.EnemyTurn()");
        gs.EnemyTurn();
    }

}
