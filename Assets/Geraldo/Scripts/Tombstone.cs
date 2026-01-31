using UnityEngine;

/// <summary>
/// Simple component attached to tombstone prefabs to allow respawning the original player later.
/// Assign the PlayerPrefab field (HealthSystem sets it at spawn time).
/// Call Respawn() to instantiate the player at the tombstone position and remove the tombstone.
/// </summary>
public class Tombstone : MonoBehaviour
{
    [HideInInspector] public GameObject PlayerPrefab;
    [HideInInspector] public string OriginalPlayerName;

    public bool CanRespawn => PlayerPrefab != null;

    public GameObject Respawn()
    {
        if (PlayerPrefab == null)
        {
            Debug.LogError("[Tombstone] Cannot respawn because PlayerPrefab is not assigned.");
            return null;
        }

        var go = Instantiate(PlayerPrefab, transform.position, Quaternion.identity, transform.parent);
        go.name = OriginalPlayerName; // try to restore original name

        // Destroy the tombstone after respawn
        Destroy(gameObject);
        return go;
    }
}
